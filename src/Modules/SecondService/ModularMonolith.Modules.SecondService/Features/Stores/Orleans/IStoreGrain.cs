using ModResults;

namespace ModularMonolith.Modules.SecondService.Features.Stores.Orleans;

internal interface IStoreGrain : IGrainWithGuidKey
{
  Task<Result<StoreEntitySurrogate>> GetStoreAsync(GrainCancellationToken gct);
  Task<Result> DeleteStoreAsync(GrainCancellationToken gct);
  Task<Result<Guid>> CreateStoreAsync(StoreEntitySurrogate store, GrainCancellationToken gct);
  Task<Result<StoreEntitySurrogate>> UpdateStoreAsync(StoreEntitySurrogate store, GrainCancellationToken gct);
  Task<Result> InvalidateStateAsync(GrainCancellationToken gct);
}
