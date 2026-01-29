using System.Diagnostics;
using System.Text.Json;
using ModularMonolith.Shared.Data.SimpleOutbox.Abstractions;
using ModularMonolith.Shared.Data.SimpleOutbox.Entities;

namespace ModularMonolith.Shared.Data.SimpleOutbox.Extensions;

public static class OutboxDbContextExtensions
{
  public static OutboxMessageEntity AddToOutbox<T>(
    this IOutboxDbContext dbContext,
    T message,
    Dictionary<string, object?>? headers = null,
    string? publisherName = null,
    TimeSpan? delay = null)
  {
    var activity = Activity.Current;
    var messageDelay = delay is null ? TimeSpan.Zero : delay;
    OutboxMessageEntityTelemetryContext? telemetryContext = activity is null
      ? null
      : new(TraceId: activity.TraceId.ToString(), SpanId: activity.SpanId.ToString());
    var entity = new OutboxMessageEntity
    {
      Content = new OutboxMessageEntityContent(
        Payload: JsonSerializer.Serialize(message),
        Type: typeof(T).AssemblyQualifiedName,
        Headers: headers),
      TelemetryContext = telemetryContext,
      PublisherName = publisherName,
      PublishAt = DateTimeOffset.UtcNow.Add(messageDelay.Value)
    };

    dbContext.OutboxMessages.Add(entity);

    return entity;
  }

  public static PublisherMessage ToPublisherMessage(this OutboxMessageEntity entity)
  {
    return new PublisherMessage(
      Id: entity.Id,
      State: entity.State,
      PublisherName: entity.PublisherName,
      TelemetryContext: entity.TelemetryContext is null
        ? null
        : new PublisherMessageTelemetryContext(
          TraceId: entity.TelemetryContext.TraceId,
          SpanId: entity.TelemetryContext.SpanId),
      Content: new PublisherMessageContent(
        Payload: entity.Content.Payload,
        Headers: entity.Content.Headers,
        Type: entity.Content.Type),
      RetryCount: entity.RetryCount,
      CreatedAt: entity.CreatedAt,
      UpdatedAt: entity.UpdatedAt,
      PublishAt: entity.PublishAt);
  }
}
