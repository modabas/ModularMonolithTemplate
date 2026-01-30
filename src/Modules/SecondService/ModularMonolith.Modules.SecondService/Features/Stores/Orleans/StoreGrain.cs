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

  protected override async Task<Result<CreateRecord<StoreEntitySurrogate>>> CreateFromStoreAsync(
    CacheGrainEntryOptions options,
    CancellationToken ct)
  {
    var idResult = this.GetPrimaryKeyAsGuid();
    if (idResult.IsFailed)
    {
      return Result<CreateRecord<StoreEntitySurrogate>>.Fail(idResult);
    }
    var id = idResult.Value;
    var db = RequestServices.GetRequiredService<SecondServiceDbContext>();

    var entity = await db.Stores.FirstOrDefaultAsync(b => b.Id == id, ct);
    if (entity is null)
    {
      return Result<CreateRecord<StoreEntitySurrogate>>.NotFound($"Store with id: {id} not found.");
    }
    return new CreateRecord<StoreEntitySurrogate>(entity.ToSurrogate(), options);
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

  protected override async Task<Result<WriteRecord<StoreEntitySurrogate>>> WriteToStoreAsync(
    StoreEntitySurrogate value,
    CacheGrainEntryOptions options,
    CancellationToken ct)
  {
    var idResult = this.GetPrimaryKeyAsGuid();
    if (idResult.IsFailed)
    {
      return Result<WriteRecord<StoreEntitySurrogate>>.Fail(idResult);
    }
    var id = idResult.Value;
    var db = RequestServices.GetRequiredService<SecondServiceDbContext>();

    var updated = await db.Stores
      .Where(b => b.Id == id)
      .ExecuteUpdateAsync(setters => setters
        .SetProperty(b => b.Name, value.Name),
      ct);

    // If no rows were updated, the entity does not exist, so we insert it
    if (updated < 1)
    {
      db.Stores.Add(value.ToEntity(id));
      db.AddToOutbox(new StoreCreatedEvent(id, value.Name));
      await db.SaveChangesAsync(ct);
    }
    return new WriteRecord<StoreEntitySurrogate>(value, options);
  }
}
