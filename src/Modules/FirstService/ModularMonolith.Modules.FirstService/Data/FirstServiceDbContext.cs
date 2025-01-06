using Microsoft.EntityFrameworkCore;
using ModularMonolith.Modules.FirstService.Features.Books.Data;
using ModularMonolith.Shared.Data;

namespace ModularMonolith.Modules.FirstService.Data;

internal class FirstServiceDbContext(DbContextOptions<FirstServiceDbContext> options)
  : PgDbContext(options)
{
  public const string Schema = "first_service";

  protected override void OnModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);

    builder.HasDefaultSchema(Schema);
    builder.ApplyConfigurationsFromAssembly(typeof(FirstServiceDbContext).Assembly);

    //Seed
  }

  #region DbSets
  public DbSet<BookEntity> Books => Set<BookEntity>();
  #endregion
}
