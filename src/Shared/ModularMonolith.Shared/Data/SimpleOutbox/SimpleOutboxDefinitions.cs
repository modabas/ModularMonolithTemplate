using ModularMonolith.Shared.Masstransit.SimpleOutbox;

namespace ModularMonolith.Shared.Data.SimpleOutbox;

public class SimpleOutboxDefinitions
{
  public const string DefaultPublisherName = MtSimpleOutboxDefinitions.PublisherName;
  public const string DefaultConfigurationKey = "SimpleOutbox";
  public const string OutboxPublisherActivitySourceName = "SimpleOutboxPublisher";
}
