namespace ModularMonolith.Modules.SecondService.FeatureContracts.Features.Stores;
public record ListStoresResponse(List<ListStoresResponseItem> Stores);
public record ListStoresResponseItem(Guid Id, string Name);
