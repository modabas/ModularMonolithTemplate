namespace ModularMonolith.Modules.SecondService.IntegrationContracts.Integrations.Stores;

public record StoreCreatedEvent(
  Guid Id,
  string Name);
