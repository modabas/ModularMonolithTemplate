namespace ModularMonolith.Shared.IntegrationContracts.SecondService.Stores;

public record StoreCreatedEvent(
  Guid Id,
  string Name);
