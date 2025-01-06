using ModResults;
using ModularMonolith.Shared.Data.SimpleOutbox.Entities;

namespace ModularMonolith.Shared.Data.SimpleOutbox.Abstractions;
public interface IOutboxPublisher
{
  Task<Result> Publish(OutboxMessageEntity outboxMessage, CancellationToken cancellationToken);
}
