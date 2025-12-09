using MassTransit;
using Microsoft.Extensions.Logging;
using ModularMonolith.Shared.IntegrationContracts.FirstService.Books;

namespace ModularMonolith.Modules.SecondService.Integrations.FirstService.Books;

internal class BookCreatedConsumer(ILogger<BookCreatedConsumer> logger)
  : IConsumer<BookCreatedEvent>
{
  public Task Consume(ConsumeContext<BookCreatedEvent> context)
  {
    logger.LogInformation("Book created: {book}.", context.Message);
    return Task.CompletedTask;
  }
}
