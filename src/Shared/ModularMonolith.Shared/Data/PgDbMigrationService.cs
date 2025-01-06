using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ModularMonolith.Shared.Data;

public class PgDbMigrationServiceDefinitions
{
  public const string ActivitySourceName = "PgDbMigrationService";
}

public class PgDbMigrationService<TDbContext>(
  IServiceProvider serviceProvider,
  ILogger<PgDbMigrationService<TDbContext>> logger)
  : BackgroundService where TDbContext : DbContext
{
  private static readonly ActivitySource _activitySource = new(PgDbMigrationServiceDefinitions.ActivitySourceName);

  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    using (var activity = _activitySource.StartActivity("Running db migrations", ActivityKind.Client))
    {
      try
      {
        using (var scope = serviceProvider.CreateScope())
        {
          var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
          logger.LogInformation("Running db migrations (if necessary) for {dbType}", db.GetType());

          await RunDbMigrationsAsync(db, serviceProvider, activity, logger, cancellationToken);
        }
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Db migration error.");
        throw;
      }
    }
  }

  private static async Task RunDbMigrationsAsync(
    TDbContext db,
    IServiceProvider serviceProvider,
    Activity? activity,
    ILogger<PgDbMigrationService<TDbContext>> logger,
    CancellationToken cancellationToken)
  {
    var lockId = GetLockId(db);
    activity?.SetTag("migration.lock.id", lockId);
    await using (var transaction = await db.Database.BeginTransactionAsync(cancellationToken))
    {
      // Avoid concurrent runs by acquiring transactional advisory lock
      // Transactional advisory lock is released when transaction completes
      var lockAcquired = await db.Database
        .SqlQuery<bool>($"SELECT pg_try_advisory_xact_lock({lockId}) AS \"Value\" ")
        //This OrderBy is to prevent EF warning:
        //"The query uses the 'First'/'FirstOrDefault' operator without 'OrderBy' and filter operators. This may lead to unpredictable results."
        .OrderBy(x => x)
        .FirstOrDefaultAsync(cancellationToken);
      activity?.SetTag("migration.lock.acquired", lockAcquired);
      // Run db migrations if lock is acquired
      if (lockAcquired)
      {
        //fix: "connection already open"
        using (var scope = serviceProvider.CreateScope())
        {
          var dbContext2 = scope.ServiceProvider.GetRequiredService<TDbContext>();
          await dbContext2.Database.MigrateAsync(cancellationToken);
        }
      }
    }
  }


  private static long GetLockId(TDbContext db)
  {
    var serviceName = db.GetType().Assembly.FullName;
    if (string.IsNullOrEmpty(serviceName))
      return 0;
    return GetInt64Hash(serviceName);
  }

  private static long GetInt64Hash(string text)
  {
    using (var hasher = SHA1.Create())
    {
      var bytes = hasher.ComputeHash(Encoding.Default.GetBytes(text));
      Array.Resize(ref bytes, bytes.Length + bytes.Length % 8); //make multiple of 8 if hash is not, for example SHA1 creates 20 bytes. 
      return Enumerable.Range(0, bytes.Length / 8) // create a counter for de number of 8 bytes in the bytearray
          .Select(i => BitConverter.ToInt64(bytes, i * 8)) // combine 8 bytes at a time into a integer
          .Aggregate((x, y) => x ^ y); //xor the bytes together so you end up with a ulong (64-bit int)
    }
  }
}
