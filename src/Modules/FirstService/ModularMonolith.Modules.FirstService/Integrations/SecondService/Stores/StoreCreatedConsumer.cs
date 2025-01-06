using MassTransit;
using Microsoft.Extensions.Logging;
using ModularMonolith.Modules.SecondService.IntegrationContracts.Integrations.Stores;

namespace ModularMonolith.Modules.FirstService.Integrations.SecondService.Stores;
internal class StoreCreatedConsumer(ILogger<StoreCreatedConsumer> logger)
  : IConsumer<StoreCreatedEvent>
{
  public Task Consume(ConsumeContext<StoreCreatedEvent> context)
  {
    logger.LogInformation("Store created: {Store}.", context.Message);
    return Task.CompletedTask;
  }
}
