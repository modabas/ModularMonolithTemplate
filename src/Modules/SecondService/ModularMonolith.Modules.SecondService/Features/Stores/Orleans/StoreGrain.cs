using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModResults;
using ModularMonolith.Modules.SecondService.Data;
using ModularMonolith.Modules.SecondService.IntegrationContracts.Integrations.Stores;
using ModularMonolith.Shared.Data.SimpleOutbox.Extensions;
using ModularMonolith.Shared.Orleans;

namespace ModularMonolith.Modules.SecondService.Features.Stores.Orleans;

internal class StoreGrain : BaseGrain, IStoreGrain
{
  private StoreGrainState? _state;

  public async Task<Result<Guid>> CreateStoreAsync(StoreGrainState store, GrainCancellationToken gct)
  {
    var id = this.GetPrimaryKey();
    var ct = gct.CancellationToken;
    var db = RequestServices.GetRequiredService<SecondServiceDbContext>();

    db.Stores.Add(store.ToEntity(id));
    db.AddToOutbox(new StoreCreatedEvent(id, store.Name));
    await db.SaveChangesAsync(ct);

    //set in-memory
    _state = store;

    return id;
  }

  public async Task<Result> DeleteStoreAsync(GrainCancellationToken gct)
  {
    var id = this.GetPrimaryKey();
    var ct = gct.CancellationToken;
    var db = RequestServices.GetRequiredService<SecondServiceDbContext>();

    var deleted = await db.Stores.Where(b => b.Id == id).ExecuteDeleteAsync(ct);

    //set in-memory
    _state = null;

    return deleted > 0 ? Result.Ok() : Result.NotFound();
  }

  public async Task<Result<StoreGrainState>> GetStoreAsync(GrainCancellationToken gct)
  {
    var id = this.GetPrimaryKey();
    var ct = gct.CancellationToken;
    var db = RequestServices.GetRequiredService<SecondServiceDbContext>();

    //check in-memory
    if (_state is null)
    {
      var entity = await db.Stores.FirstOrDefaultAsync(s => s.Id == id, ct);

      //set in-memory
      _state = entity?.ToState();
    }

    //create event
    db.AddToOutbox(new StoreQueriedByIdEvent(id));
    await db.SaveChangesAsync(ct);

    var result = _state is null ?
      Result<StoreGrainState>.NotFound(string.Format("Store with id: {0} not found.", id)) :
      _state;
    return result;
  }

  public Task<Result> InvalidateStateAsync(GrainCancellationToken gct)
  {
    _state = null;
    return Task.FromResult(Result.Ok());
  }

  public async Task<Result<StoreGrainState>> UpdateStoreAsync(StoreGrainState book, GrainCancellationToken gct)
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
      _state = book;
      return book;
    }
    else
    {
      return Result<StoreGrainState>.NotFound();
    }
  }
}
