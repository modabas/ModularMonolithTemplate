namespace ModularMonolith.Shared.Data.SimpleOutbox;

public class SimpleOutboxSettings
{
  public TimeSpan PublisherTimerPeriod { get; set; } = TimeSpan.FromSeconds(1);
  public int PublisherBatchCount { get; set; } = 10;
  public int PublisherRetryCount { get; set; } = 100;
  public TimeSpan PublisherRetryInterval { get; set; } = TimeSpan.FromMinutes(2);
  public TimeSpan PublisherTimeout { get; set; } = TimeSpan.FromSeconds(5);

  public TimeSpan OutboxRemovalTimerPeriod { get; set; } = TimeSpan.FromDays(1);
  public int OutboxRemovalBeforeInDays { get; set; } = 5;

  public string DefaultPublisherName { get; set; } = SimpleOutboxDefinitions.DefaultPublisherName;
}
