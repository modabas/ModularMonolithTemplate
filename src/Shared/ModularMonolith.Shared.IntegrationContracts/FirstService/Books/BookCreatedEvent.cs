namespace ModularMonolith.Shared.IntegrationContracts.FirstService.Books;

public record BookCreatedEvent(
  Guid Id,
  string Title,
  string Author,
  decimal Price);
