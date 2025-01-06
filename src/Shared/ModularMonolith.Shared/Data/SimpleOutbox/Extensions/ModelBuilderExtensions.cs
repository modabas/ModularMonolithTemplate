using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Data.SimpleOutbox.Entities;

namespace ModularMonolith.Shared.Data.SimpleOutbox.Extensions;

public static class ModelBuilderExtensions
{
  public static ModelBuilder ConfigureOutboxMessageEntity(this ModelBuilder modelBuilder)
  {
    var entity = modelBuilder.Entity<OutboxMessageEntity>();

    entity
      .HasIndex(x => new { x.State, x.RetryCount, x.RetryAt })
      .IsUnique(false);

    return modelBuilder;
  }
}
