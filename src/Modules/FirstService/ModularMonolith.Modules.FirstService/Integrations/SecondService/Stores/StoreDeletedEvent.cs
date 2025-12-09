using MassTransit;
using Microsoft.Extensions.Logging;
using ModularMonolith.Shared.IntegrationContracts.SecondService.Stores;

namespace ModularMonolith.Modules.FirstService.Integrations.SecondService.Stores;

internal class StoreDeletedConsumer(ILogger<StoreDeletedConsumer> logger)
  : IConsumer<StoreDeletedEvent>
{
  public Task Consume(ConsumeContext<StoreDeletedEvent> context)
  {
    logger.LogInformation("Store deleted: {Store}.", context.Message);
    return Task.CompletedTask;
  }
}
