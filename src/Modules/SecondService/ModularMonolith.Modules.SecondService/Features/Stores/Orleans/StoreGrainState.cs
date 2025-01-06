namespace ModularMonolith.Modules.SecondService.Features.Stores.Orleans;

[GenerateSerializer]
internal record StoreGrainState(
  string Name);
