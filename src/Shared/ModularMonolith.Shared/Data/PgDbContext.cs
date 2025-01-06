using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Data.SimpleOutbox.Abstractions;
using ModularMonolith.Shared.Data.SimpleOutbox.Entities;
using ModularMonolith.Shared.Data.SimpleOutbox.Extensions;

namespace ModularMonolith.Shared.Data;

public abstract class PgDbContext(DbContextOptions options)
  : DbContext(options), IOutboxDbContext
{
  protected override void OnModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);

    builder.ConfigureOutboxMessageEntity();
  }

  public DbSet<OutboxMessageEntity> OutboxMessages => Set<OutboxMessageEntity>();
}
