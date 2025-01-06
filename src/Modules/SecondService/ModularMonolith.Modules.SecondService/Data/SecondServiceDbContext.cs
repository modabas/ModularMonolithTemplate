using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.SecondService.Features.Stores.Data;
using ModularMonolith.Shared.Data;

namespace ModularMonolith.Modules.SecondService.Data;

internal class SecondServiceDbContext(DbContextOptions<SecondServiceDbContext> options)
  : PgDbContext(options)
{
  public const string Schema = "second_service";

  protected override void OnModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);

    builder.HasDefaultSchema(Schema);
    builder.ApplyConfigurationsFromAssembly(typeof(SecondServiceDbContext).Assembly);

    //Seed
  }

  #region DbSets
  public DbSet<StoreEntity> Stores => Set<StoreEntity>();
  #endregion
}
