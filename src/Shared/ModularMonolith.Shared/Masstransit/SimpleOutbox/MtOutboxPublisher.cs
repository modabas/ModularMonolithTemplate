using System.Text.Json;
using MassTransit;
using ModResults;
using ModularMonolith.Shared.Data.SimpleOutbox.Abstractions;
using ModularMonolith.Shared.Data.SimpleOutbox.Entities;

namespace ModularMonolith.Shared.Masstransit.SimpleOutbox;
public class MtOutboxPublisher(IPublishEndpoint publishEndpoint) : IOutboxPublisher
{
  public const string OutboxMessageId = "OutboxMessageId";

  public async Task<Result> Publish(OutboxMessageEntity outboxMessage, CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(outboxMessage.Type))
    {
      return Result.Error("message.Type is null or whitespace.");
    }

    var type = Type.GetType(outboxMessage.Type);
    if (type is null)
    {
      return Result.Error($"Cannot resolve message type from string {outboxMessage.Type}");
    }

    var messageObject = JsonSerializer.Deserialize(outboxMessage.Payload, type);
    if (messageObject is null)
    {
      return Result.Error("message.Payload cannot be deserialized as a message.Type object.");
    }

    Dictionary<string, object?>? headers = null;
    if (outboxMessage.Headers is not null)
    {
      headers = JsonSerializer.Deserialize<Dictionary<string, object?>>(outboxMessage.Headers);
    }
    await publishEndpoint.Publish(messageObject, type,
      x =>
      {
        if (headers is not null)
        {
          foreach (var header in headers)
          {
            x.Headers.Set(header.Key, header.Value, true);
          }
        }
        x.Headers.Set(OutboxMessageId, outboxMessage.Id, true);
      },
      cancellationToken);
    return Result.Ok();
  }
}
