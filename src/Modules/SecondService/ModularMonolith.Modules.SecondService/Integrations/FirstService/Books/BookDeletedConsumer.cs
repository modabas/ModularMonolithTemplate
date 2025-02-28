﻿using MassTransit;
using Microsoft.Extensions.Logging;
using ModularMonolith.Shared.IntegrationContracts.FirstService.Books;

namespace ModularMonolith.Modules.SecondService.Integrations.FirstService.Books;
internal class BookDeletedConsumer(ILogger<BookDeletedConsumer> logger)
  : IConsumer<BookDeletedEvent>
{
  public Task Consume(ConsumeContext<BookDeletedEvent> context)
  {
    logger.LogInformation("Book deleted: {book}.", context.Message);
    return Task.CompletedTask;
  }
}
