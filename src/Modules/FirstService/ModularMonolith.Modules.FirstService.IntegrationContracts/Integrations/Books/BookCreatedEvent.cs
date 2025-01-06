namespace ModularMonolith.Modules.FirstService.IntegrationContracts.Integrations.Books;

public record BookCreatedEvent(
  Guid Id,
  string Title,
  string Author,
  decimal Price);
