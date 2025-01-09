using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModResults;
using ModularMonolith.Modules.FirstService.IntegrationContracts.Integrations.Books;
using ModularMonolith.Modules.SecondService.Data;
using ModularMonolith.Modules.SecondService.IntegrationContracts.Integrations.Stores;
using ModularMonolith.Shared.Data.SimpleOutbox.Extensions;
using ModularMonolith.Shared.Orleans;

namespace ModularMonolith.Modules.SecondService.Features.Stores.Orleans;

internal class StoreGrain : BaseGrain, IStoreGrain
{
  private StoreEntitySurrogate? _cache;

  public async Task<Result<Guid>> CreateStoreAsync(StoreEntitySurrogate store, GrainCancellationToken gct)
  {
    var id = this.GetPrimaryKey();
    var ct = gct.CancellationToken;
    var db = RequestServices.GetRequiredService<SecondServiceDbContext>();

    db.Stores.Add(store.ToEntity(id));
    db.AddToOutbox(new StoreCreatedEvent(id, store.Name));
    await db.SaveChangesAsync(ct);

    //set in-memory
    _cache = store;

    return id;
  }

  public async Task<Result> DeleteStoreAsync(GrainCancellationToken gct)
  {
    var id = this.GetPrimaryKey();
    var ct = gct.CancellationToken;
    var db = RequestServices.GetRequiredService<SecondServiceDbContext>();

    var entity = await db.Stores.FirstOrDefaultAsync(s => s.Id == id, ct);
    if (entity is null)
    {
      //set in-memory
      _cache = null;
      return Result.NotFound();
    }
    db.Remove(entity);
    db.AddToOutbox(new StoreDeletedEvent(id));
    await db.SaveChangesAsync(ct);

    //set in-memory
    _cache = null;

    return Result.Ok();
  }

  public async Task<Result<StoreEntitySurrogate>> GetStoreAsync(GrainCancellationToken gct)
  {
    var id = this.GetPrimaryKey();
    var ct = gct.CancellationToken;
    var db = RequestServices.GetRequiredService<SecondServiceDbContext>();

    //check in-memory
    if (_cache is null)
    {
      var entity = await db.Stores.FirstOrDefaultAsync(s => s.Id == id, ct);

      //set in-memory
      _cache = entity?.ToSurrogate();
    }

    var result = _cache is null ?
      Result<StoreEntitySurrogate>.NotFound(string.Format("Store with id: {0} not found.", id)) :
      _cache;
    return result;
  }

  public Task<Result> InvalidateStateAsync(GrainCancellationToken gct)
  {
    _cache = null;
    return Task.FromResult(Result.Ok());
  }

  public async Task<Result<StoreEntitySurrogate>> UpdateStoreAsync(StoreEntitySurrogate book, GrainCancellationToken gct)
  {
    var id = this.GetPrimaryKey();
    var ct = gct.CancellationToken;
    var db = RequestServices.GetRequiredService<SecondServiceDbContext>();

    var updated = await db.Stores
      .Where(b => b.Id == id)
      .ExecuteUpdateAsync(setters => setters
        .SetProperty(b => b.Name, book.Name),
      ct);

    if (updated > 0)
    {
      //set in-memory
      _cache = book;
      return book;
    }
    else
    {
      return Result<StoreEntitySurrogate>.NotFound();
    }
  }
}
