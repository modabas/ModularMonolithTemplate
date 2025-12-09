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
    var entity = new OutboxMessageEntity
    {
      Payload = JsonSerializer.Serialize(message),
      Type = typeof(T).AssemblyQualifiedName,
      SpanId = activity?.SpanId.ToString(),
      TraceId = activity?.TraceId.ToString(),
      Headers = headers is null ? null : JsonSerializer.Serialize(headers),
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
      TraceId: entity.TraceId,
      SpanId: entity.SpanId,
      Payload: entity.Payload,
      Headers: entity.Headers,
      Type: entity.Type,
      RetryCount: entity.RetryCount,
      CreatedAt: entity.CreatedAt,
      UpdatedAt: entity.UpdatedAt,
      PublishAt: entity.PublishAt);
  }
}
