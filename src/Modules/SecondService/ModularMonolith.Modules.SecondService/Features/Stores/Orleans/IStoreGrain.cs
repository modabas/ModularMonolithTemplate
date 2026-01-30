using ModCaches.Orleans.Abstractions.Cluster;

namespace ModularMonolith.Modules.SecondService.Features.Stores.Orleans;

internal interface IStoreGrain
  : IReadThroughCacheGrain<StoreEntitySurrogate>,
  IWriteThroughCacheGrain<StoreEntitySurrogate>
{
}

