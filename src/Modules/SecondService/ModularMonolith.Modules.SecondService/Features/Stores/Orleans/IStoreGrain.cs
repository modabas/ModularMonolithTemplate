using ModResults;

namespace ModularMonolith.Modules.SecondService.Features.Stores.Orleans;

internal interface IStoreGrain : IGrainWithGuidKey
{
  Task<Result<StoreGrainState>> GetStoreAsync(GrainCancellationToken gct);
  Task<Result> DeleteStoreAsync(GrainCancellationToken gct);
  Task<Result<Guid>> CreateStoreAsync(StoreGrainState store, GrainCancellationToken gct);
  Task<Result<StoreGrainState>> UpdateStoreAsync(StoreGrainState store, GrainCancellationToken gct);
  Task<Result> InvalidateStateAsync(GrainCancellationToken gct);
}
