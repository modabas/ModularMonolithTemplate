using ModularMonolith.Shared.Data.SimpleOutbox.Enums;

namespace ModularMonolith.Shared.Data.SimpleOutbox.Entities;

public class OutboxMessageEntity : BaseEntity
{
  public MessageState State { get; set; } = MessageState.New;

  public string? PublisherName { get; set; }
  public string? TraceId { get; set; }
  public string? SpanId { get; set; }
  public required string Payload { get; set; }
  public string? Headers { get; set; }
  public required string? Type { get; set; }

  public int RetryCount { get; set; } = 0;
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
  public DateTimeOffset? UpdatedAt { get; set; } = null;
  public DateTimeOffset PublishAt { get; set; }
}
