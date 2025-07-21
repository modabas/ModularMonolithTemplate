using ModResults;

namespace ModularMonolith.Modules.SecondService.Features.Stores.Orleans;

internal interface IStoreGrain : IGrainWithGuidKey
{
  Task<Result<StoreEntitySurrogate>> GetStoreAsync(CancellationToken ct);
  Task<Result> DeleteStoreAsync(CancellationToken ct);
  Task<Result<Guid>> CreateStoreAsync(StoreEntitySurrogate store, CancellationToken ct);
  Task<Result<StoreEntitySurrogate>> UpdateStoreAsync(StoreEntitySurrogate store, CancellationToken ct);
  Task<Result> InvalidateStateAsync(CancellationToken ct);
}
