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
    if (string.IsNullOrWhiteSpace(message.Content.Type))
    {
      return Result.Error("message.Content.Type is null or whitespace.");
    }

    var type = Type.GetType(message.Content.Type);
    if (type is null)
    {
      return Result.Error($"Cannot resolve message type from string {message.Content.Type}");
    }

    var messageObject = JsonSerializer.Deserialize(message.Content.Payload, type);
    if (messageObject is null)
    {
      return Result.Error("message.Content.Payload cannot be deserialized as a message.Content.Type object.");
    }

    var headers = message.Content.Headers;
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
