using System.Diagnostics;
using System.Text.Json;
using ModularMonolith.Shared.Data.SimpleOutbox.Abstractions;
using ModularMonolith.Shared.Data.SimpleOutbox.Entities;
using ModularMonolith.Shared.Data.SimpleOutbox.Enums;
using ModularMonolith.Shared.Guids;

namespace ModularMonolith.Shared.Data.SimpleOutbox.Extensions;

public static class OutboxDbContextExtensions
{
  public static Guid AddToOutbox<T>(this IOutboxDbContext dbContext, T message, Dictionary<string, object?>? headers = null, string? publisherName = null)
  {
    var activity = Activity.Current;
    var entity = new OutboxMessageEntity
    {
      Id = GuidV7.CreateVersion7(),
      CreatedAt = DateTimeOffset.UtcNow,
      State = MessageState.New,
      UpdatedAt = null,
      Payload = JsonSerializer.Serialize(message),
      Type = typeof(T).AssemblyQualifiedName,
      SpanId = activity?.SpanId.ToString(),
      TraceId = activity?.TraceId.ToString(),
      Headers = headers is null ? null : JsonSerializer.Serialize(headers),
      PublisherName = publisherName
    };

    dbContext.OutboxMessages.Add(entity);

    return entity.Id;
  }
}
