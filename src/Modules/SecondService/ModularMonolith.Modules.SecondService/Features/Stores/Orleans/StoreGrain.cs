using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModCaches.Orleans.Abstractions.Cluster;
using ModCaches.Orleans.Server.Cluster;
using ModResults;
using ModularMonolith.Modules.SecondService.Data;
using ModularMonolith.Shared.Data.SimpleOutbox.Extensions;
using ModularMonolith.Shared.IntegrationContracts.SecondService.Stores;
using ModularMonolith.Shared.Orleans;

namespace ModularMonolith.Modules.SecondService.Features.Stores.Orleans;

internal class StoreGrain : VolatileCacheGrain<StoreEntitySurrogate>, IStoreGrain
{
  public StoreGrain(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }

  protected override async Task<Result<ClusterCacheEntry<StoreEntitySurrogate>>> CreateFromStoreAsync(
    ClusterCacheEntryOptions options,
    CancellationToken ct)
  {
    var idResult = this.GetPrimaryKeyAsGuid();
    if (idResult.IsFailed)
    {
      return FailureResult.From(idResult);
    }
    var id = idResult.Value;
    var db = RequestServices.GetRequiredService<SecondServiceDbContext>();

    var entity = await db.Stores.FirstOrDefaultAsync(b => b.Id == id, ct);
    if (entity is null)
    {
      return Result<ClusterCacheEntry<StoreEntitySurrogate>>.NotFound($"Store with id: {id} not found.");
    }
    return ClusterCacheEntry.CreateResult(entity.ToSurrogate(), options);
  }

  protected override async Task<Result> DeleteFromStoreAsync(CancellationToken ct)
  {
    var idResult = this.GetPrimaryKeyAsGuid();
    if (idResult.IsFailed)
    {
      return idResult;
    }
    var id = idResult.Value;
    var db = RequestServices.GetRequiredService<SecondServiceDbContext>();

    using (var transaction = await db.Database.BeginTransactionAsync(ct))
    {
      var deleted = await db.Stores
        .Where(b => b.Id == id)
        .ExecuteDeleteAsync(ct);
      if (deleted > 0)
      {
        db.AddToOutbox(new StoreDeletedEvent(id));
        await db.SaveChangesAsync(ct);
      }
      await transaction.CommitAsync(ct);
    }
    return Result.Ok();
  }

  protected override async Task<Result<ClusterCacheEntry<StoreEntitySurrogate>>> WriteToStoreAsync(
    StoreEntitySurrogate value,
    ClusterCacheEntryOptions options,
    CancellationToken ct)
  {
    var idResult = this.GetPrimaryKeyAsGuid();
    if (idResult.IsFailed)
    {
      return FailureResult.From(idResult);
    }
    var id = idResult.Value;
    var db = RequestServices.GetRequiredService<SecondServiceDbContext>();

    var updated = await db.Stores
      .Where(b => b.Id == id)
      .ExecuteUpdateAsync(setters => setters
        .SetProperty(b => b.Name, value.Name),
      ct);

    // If no rows were updated, the entity does not exist, return NotFound
    if (updated < 1)
    {
      return Result<ClusterCacheEntry<StoreEntitySurrogate>>.NotFound($"Store with id: {id} not found.");
    }
    return ClusterCacheEntry.CreateResult(value, options);
  }
}
