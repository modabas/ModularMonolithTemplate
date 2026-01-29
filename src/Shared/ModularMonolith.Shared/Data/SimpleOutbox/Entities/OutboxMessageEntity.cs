using ModularMonolith.Shared.Data.SimpleOutbox.Enums;

namespace ModularMonolith.Shared.Data.SimpleOutbox.Entities;

public class OutboxMessageEntity : BaseEntity
{
  public MessageState State { get; set; } = MessageState.New;

  public string? PublisherName { get; set; }
  public OutboxMessageEntityTelemetryContext? TelemetryContext { get; set; }

  public required OutboxMessageEntityContent Content { get; set; }

  public int RetryCount { get; set; } = 0;
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
  public DateTimeOffset? UpdatedAt { get; set; } = null;
  public DateTimeOffset PublishAt { get; set; }
}

public record OutboxMessageEntityTelemetryContext(string TraceId, string SpanId);

public record OutboxMessageEntityContent(string Payload, Dictionary<string, object?>? Headers, string? Type);
