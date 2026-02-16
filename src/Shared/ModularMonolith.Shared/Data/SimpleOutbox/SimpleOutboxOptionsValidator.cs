using FluentValidation;

namespace ModularMonolith.Shared.Data.SimpleOutbox;

public class SimpleOutboxOptionsValidator : AbstractValidator<SimpleOutboxOptions>
{
  public SimpleOutboxOptionsValidator()
  {
    RuleFor(x => x.PublisherTimerPeriod).GreaterThan(TimeSpan.Zero);
    RuleFor(x => x.PublisherBatchCount).GreaterThan(0);
    RuleFor(x => x.PublisherRetryCount).GreaterThanOrEqualTo(0);
    RuleFor(x => x.PublisherRetryInterval).GreaterThan(TimeSpan.Zero);
    RuleFor(x => x.PublisherTimeout).GreaterThan(TimeSpan.Zero);
    RuleFor(x => x.OutboxRemovalTimerPeriod).GreaterThan(TimeSpan.Zero);
    RuleFor(x => x.OutboxRemovalBeforeInDays).GreaterThanOrEqualTo(0);
    RuleFor(x => x.DefaultPublisherName).NotEmpty();
  }
}
