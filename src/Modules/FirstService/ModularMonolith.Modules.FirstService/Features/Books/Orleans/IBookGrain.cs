using ModCaches.Orleans.Abstractions.Cluster;

namespace ModularMonolith.Modules.FirstService.Features.Books.Orleans;

internal interface IBookGrain
  : IReadThroughCacheGrain<BookEntitySurrogate>,
  IWriteThroughCacheGrain<BookEntitySurrogate>
{
}
