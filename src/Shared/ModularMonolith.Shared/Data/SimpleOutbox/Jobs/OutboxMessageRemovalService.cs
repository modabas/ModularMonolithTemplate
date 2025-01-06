using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.Data.SimpleOutbox.Abstractions;
using ModularMonolith.Shared.Data.SimpleOutbox.Enums;

namespace ModularMonolith.Shared.Data.SimpleOutbox.Jobs;

public class OutboxMessageRemovalService<TDbContext>(
   IServiceScopeFactory serviceScopeFactory,
   ILogger<OutboxMessagePublisherService<TDbContext>> logger,
   IOptions<SimpleOutboxSettings> options) : BackgroundService
    where TDbContext : DbContext, IOutboxDbContext
{
  protected override async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    var settings = options.Value;
    using (var timer = new PeriodicTimer(settings.OutboxRemovalTimerPeriod))
    {
      while (await timer.WaitForNextTickAsync(cancellationToken))
      {
        try
        {
          logger.LogDebug($"{nameof(OutboxMessageRemovalService<TDbContext>)} started iteration");

          using (var scope = serviceScopeFactory.CreateScope())
          {
            var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
            var daysBefore = DateTime.UtcNow.AddDays(-settings.OutboxRemovalBeforeInDays);

            await dbContext.OutboxMessages
                .Where(x => x.State == MessageState.Done ||
                  x.State == MessageState.Errored && x.RetryCount > settings.PublisherRetryCount)
                .Where(x => x.UpdatedAt < daysBefore)
                .ExecuteDeleteAsync(cancellationToken);
          }
          logger.LogDebug($"{nameof(OutboxMessageRemovalService<TDbContext>)} finished iteration");
        }
        catch (Exception ex)
        {
          logger.LogError(ex, "Error outbox clean-up loop.");
        }
      }
    }
  }
}
