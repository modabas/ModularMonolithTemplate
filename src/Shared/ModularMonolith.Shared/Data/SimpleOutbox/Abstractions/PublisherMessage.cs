using ModularMonolith.Shared.Data.SimpleOutbox.Enums;

namespace ModularMonolith.Shared.Data.SimpleOutbox.Abstractions;

public record PublisherMessage(
  Guid Id,
  MessageState State,
  string? PublisherName,
  string? TraceId,
  string? SpanId,
  string Payload,
  string? Headers,
  string? Type,
  int RetryCount,
  DateTimeOffset CreatedAt,
  DateTimeOffset? UpdatedAt,
  DateTimeOffset PublishAt);
