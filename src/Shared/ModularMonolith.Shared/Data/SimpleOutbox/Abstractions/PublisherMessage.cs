using ModularMonolith.Shared.Data.SimpleOutbox.Enums;

namespace ModularMonolith.Shared.Data.SimpleOutbox.Abstractions;

public record PublisherMessage(
  Guid Id,
  MessageState State,
  string? PublisherName,
  PublisherMessageTelemetryContext? TelemetryContext,
  PublisherMessageContent Content,
  int RetryCount,
  DateTimeOffset CreatedAt,
  DateTimeOffset? UpdatedAt,
  DateTimeOffset PublishAt);

public record PublisherMessageTelemetryContext(string? TraceId, string? SpanId);

public record PublisherMessageContent(string Payload, Dictionary<string, object?>? Headers, string? Type);

