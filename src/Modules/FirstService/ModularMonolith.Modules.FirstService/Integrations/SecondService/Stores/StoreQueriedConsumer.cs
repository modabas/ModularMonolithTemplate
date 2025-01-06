using MassTransit;
using Microsoft.Extensions.Logging;
using ModularMonolith.Modules.SecondService.IntegrationContracts.Integrations.Stores;

namespace ModularMonolith.Modules.FirstService.Integrations.SecondService.Stores;
internal class StoreQueriedConsumer(ILogger<StoreQueriedConsumer> logger)
  : IConsumer<StoreQueriedByIdEvent>
{
  public Task Consume(ConsumeContext<StoreQueriedByIdEvent> context)
  {
    logger.LogInformation("Store queried by id: {query}.", context.Message);
    return Task.CompletedTask;
  }
}
