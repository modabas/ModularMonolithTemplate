using System.Text.Json;
using MassTransit;
using ModResults;
using ModularMonolith.Shared.Data.SimpleOutbox.Abstractions;

namespace ModularMonolith.Shared.Masstransit.SimpleOutbox;

public class MtOutboxPublisher(IPublishEndpoint publishEndpoint) : IOutboxPublisher
{
  public const string OutboxMessageId = "OutboxMessageId";

  public async Task<Result> Publish(PublisherMessage message, CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(message.Type))
    {
      return Result.Error("message.Type is null or whitespace.");
    }

    var type = Type.GetType(message.Type);
    if (type is null)
    {
      return Result.Error($"Cannot resolve message type from string {message.Type}");
    }

    var messageObject = JsonSerializer.Deserialize(message.Payload, type);
    if (messageObject is null)
    {
      return Result.Error("message.Payload cannot be deserialized as a message.Type object.");
    }

    Dictionary<string, object?>? headers = null;
    if (message.Headers is not null)
    {
      headers = JsonSerializer.Deserialize<Dictionary<string, object?>>(message.Headers);
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
        x.Headers.Set(OutboxMessageId, message.Id, true);
      },
      cancellationToken);
    return Result.Ok();
  }
}
