using MassTransit;
using Microsoft.Extensions.Logging;
using ModularMonolith.Modules.FirstService.IntegrationContracts.Integrations.Books;

namespace ModularMonolith.Modules.SecondService.Integrations.FirstService.Books;
internal class BookQueriedConsumer(ILogger<BookQueriedConsumer> logger)
  : IConsumer<BookQueriedByIdEvent>
{
  public Task Consume(ConsumeContext<BookQueriedByIdEvent> context)
  {
    logger.LogInformation("Book queried by id: {query}.", context.Message);
    return Task.CompletedTask;
  }
}
