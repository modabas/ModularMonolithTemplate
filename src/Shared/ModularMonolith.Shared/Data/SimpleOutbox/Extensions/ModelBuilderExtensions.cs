using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Data.SimpleOutbox.Entities;

namespace ModularMonolith.Shared.Data.SimpleOutbox.Extensions;

public static class ModelBuilderExtensions
{
  public static ModelBuilder ConfigureOutboxMessageEntity(this ModelBuilder modelBuilder)
  {
    var entity = modelBuilder.Entity<OutboxMessageEntity>();

    entity
      .HasIndex(x => new { x.State, x.RetryCount, x.PublishAt })
      .IsUnique(false);
    entity.ComplexProperty(e => e.Content,
      e =>
      {
        e.Property(e => e.Headers)
        .HasConversion(
          v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v),
          v => v == null ? null : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(v))
        .HasColumnType("jsonb");

        e.Property(e => e.Payload)
        .HasColumnType("jsonb");
      });
    entity.ComplexProperty(e => e.TelemetryContext, e => e.ToJson());

    return modelBuilder;
  }
}
