using ModResults;

namespace ModularMonolith.Shared.Data.SimpleOutbox.Abstractions;

public interface IOutboxPublisher
{
  Task<Result> Publish(PublisherMessage message, CancellationToken cancellationToken);
}
