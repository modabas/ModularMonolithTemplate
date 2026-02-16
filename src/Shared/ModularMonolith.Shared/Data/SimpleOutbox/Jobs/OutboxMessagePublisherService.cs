using System.Data;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.Data.QueryLockHints.Enums;
using ModularMonolith.Shared.Data.QueryLockHints.Extensions;
using ModularMonolith.Shared.Data.SimpleOutbox.Abstractions;
using ModularMonolith.Shared.Data.SimpleOutbox.Enums;
using ModularMonolith.Shared.Data.SimpleOutbox.Extensions;

namespace ModularMonolith.Shared.Data.SimpleOutbox.Jobs;

public class OutboxMessagePublisherService<TDbContext>(
   IServiceScopeFactory serviceScopeFactory,
   ILogger<OutboxMessagePublisherService<TDbContext>> logger,
   IOptions<SimpleOutboxOptions> options) : BackgroundService
    where TDbContext : DbContext, IOutboxDbContext
{
  private static readonly ActivitySource _activitySource = new(SimpleOutboxDefinitions.OutboxPublisherActivitySourceName);

  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    var settings = options.Value;
    var fetchedMessageCount = int.MinValue;
    using (var timer = new PeriodicTimer(settings.PublisherTimerPeriod))
    {
      while (fetchedMessageCount == settings.PublisherBatchCount ||
        await timer.WaitForNextTickAsync(cancellationToken))
      {
        try
        {
          using (var pollActivity = _activitySource.StartActivity(
            ActivityKind.Internal,
            name: $"OutboxMessagePublisherService of {typeof(TDbContext)} Polling"))
          {
            using (var scope = serviceScopeFactory.CreateScope())
            {
              var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
              using (var transaction = await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted, cancellationToken))
              {
                try
                {
                  var utcNow = DateTimeOffset.UtcNow;
                  var messages = await dbContext.OutboxMessages
                    .Where(x => x.State == MessageState.New && x.RetryCount <= settings.PublisherRetryCount && x.PublishAt <= utcNow ||
                      x.State == MessageState.Errored && x.RetryCount <= settings.PublisherRetryCount && x.PublishAt <= utcNow)
                    .OrderBy(x => x.Id)
                    .WithQueryLock(QueryLockStrength.Update, QueryLockBehavior.SkipLocked)
                    .Take(settings.PublisherBatchCount)
                    .ToListAsync(cancellationToken);

                  fetchedMessageCount = messages.Count;

                  if (fetchedMessageCount == 0)
                  {
                    continue;
                  }

                  await PublishMessagesAsync(settings, scope, dbContext, messages, cancellationToken);
                  await dbContext.SaveChangesAsync(cancellationToken);
                  await transaction.CommitAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                  logger.LogError(ex, "Error processing publish batch.");
                  await transaction.RollbackAsync(cancellationToken);
                }
              }
            }
            logger.LogDebug($"{nameof(OutboxMessagePublisherService<TDbContext>)} finished iteration");
          }
        }
        catch (Exception ex)
        {
          logger.LogError(ex, "Error outbox publish loop.");
        }
      }
    }
  }

  private async Task PublishMessagesAsync(
    SimpleOutboxOptions settings,
    IServiceScope scope,
    TDbContext dbContext,
    List<Entities.OutboxMessageEntity> messages,
    CancellationToken ct)
  {
    var publishedMessageIds = new List<Guid>();
    var erroredMessageIds = new List<Guid>();

    foreach (var message in messages)
    {
      try
      {
        var telemetryContext = message.TelemetryContext;
        ActivityContext parentContext = default;
        if (telemetryContext is not null)
        {
          parentContext = new ActivityContext(
            traceId: ActivityTraceId.CreateFromString(telemetryContext.TraceId),
            spanId: ActivitySpanId.CreateFromString(telemetryContext.SpanId),
            traceFlags: ActivityTraceFlags.None);
        }
        using (var activity = _activitySource.StartActivity(
          ActivityKind.Producer,
          parentContext: parentContext,
          name: "Publishing Outbox Message"))
        {
          var publisherKey = message.PublisherName ?? settings.DefaultPublisherName;
          var outboxPublisher = scope.ServiceProvider.GetRequiredKeyedService<IOutboxPublisher>(publisherKey);
          using (var cts = CancellationTokenSource.CreateLinkedTokenSource(ct))
          {
            cts.CancelAfter(settings.PublisherTimeout);

            var publishResult = await outboxPublisher.Publish(message.ToPublisherMessage(), cts.Token);

            if (publishResult.IsOk)
            {
              publishedMessageIds.Add(message.Id);
            }
            else
            {
              logger.LogError("Error publishing message with {messageId}: {errorMessages}", message.Id, string.Join(Environment.NewLine, publishResult.Failure.Errors.Select(e => e.Message)));
              erroredMessageIds.Add(message.Id);
            }
          }
        }
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Error publishing message with {messageId}", message.Id);
        erroredMessageIds.Add(message.Id);
      }
    }
    if (publishedMessageIds.Count > 0)
    {
      var utcNow = DateTimeOffset.UtcNow;
      await dbContext.OutboxMessages
          .Where(b => publishedMessageIds.Contains(b.Id))
          .ExecuteUpdateAsync(x => x.SetProperty(m => m.State, MessageState.Done)
              .SetProperty(m => m.UpdatedAt, utcNow), cancellationToken: ct);
    }

    if (erroredMessageIds.Count > 0)
    {
      var utcNow = DateTimeOffset.UtcNow;
      var utcRetryAt = utcNow.Add(settings.PublisherRetryInterval);
      await dbContext.OutboxMessages
          .Where(b => erroredMessageIds.Contains(b.Id))
          .ExecuteUpdateAsync(x => x.SetProperty(m => m.State, MessageState.Errored)
              .SetProperty(m => m.UpdatedAt, utcNow)
              .SetProperty(m => m.RetryCount, m => m.RetryCount + 1)
              .SetProperty(m => m.PublishAt, utcRetryAt), cancellationToken: ct);
    }
  }
}
