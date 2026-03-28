using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModCaches.Orleans.Abstractions.Cluster;
using ModCaches.Orleans.Server.Cluster;
using ModResults;
using ModularMonolith.Modules.FirstService.Data;
using ModularMonolith.Shared.Data.SimpleOutbox.Extensions;
using ModularMonolith.Shared.IntegrationContracts.FirstService.Books;
using ModularMonolith.Shared.Orleans;

namespace ModularMonolith.Modules.FirstService.Features.Books.Orleans;

internal class BookGrain : VolatileCacheGrain<BookEntitySurrogate>, IBookGrain
{
  public BookGrain(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }

  protected override async Task<Result<ClusterCacheEntry<BookEntitySurrogate>>> CreateFromStoreAsync(
    ClusterCacheEntryOptions options,
    CancellationToken ct)
  {
    var idResult = this.GetPrimaryKeyAsGuid();
    if (idResult.IsFailed)
    {
      return FailureResult.From(idResult);
    }
    var id = idResult.Value;
    var db = RequestServices.GetRequiredService<FirstServiceDbContext>();

    var entity = await db.Books.FirstOrDefaultAsync(b => b.Id == id, ct);
    if (entity is null)
    {
      return Result<ClusterCacheEntry<BookEntitySurrogate>>.NotFound($"Book with id: {id} not found.");
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
    var db = RequestServices.GetRequiredService<FirstServiceDbContext>();

    using (var transaction = await db.Database.BeginTransactionAsync(ct))
    {
      var deleted = await db.Books
        .Where(b => b.Id == id)
        .ExecuteDeleteAsync(ct);
      if (deleted > 0)
      {
        db.AddToOutbox(new BookDeletedEvent(id));
        await db.SaveChangesAsync(ct);
      }
      await transaction.CommitAsync(ct);
    }
    return Result.Ok();
  }

  protected override async Task<Result<ClusterCacheEntry<BookEntitySurrogate>>> WriteToStoreAsync(
    BookEntitySurrogate value,
    ClusterCacheEntryOptions options,
    CancellationToken ct)
  {
    var idResult = this.GetPrimaryKeyAsGuid();
    if (idResult.IsFailed)
    {
      return FailureResult.From(idResult);
    }
    var id = idResult.Value;
    var db = RequestServices.GetRequiredService<FirstServiceDbContext>();

    var updated = await db.Books
      .Where(b => b.Id == id)
      .ExecuteUpdateAsync(setters => setters
        .SetProperty(b => b.Title, value.Title)
        .SetProperty(b => b.Author, value.Author)
        .SetProperty(b => b.Price, value.Price),
      ct);

    // If no rows were updated, the entity does not exist, return NotFound
    if (updated < 1)
    {
      return Result<ClusterCacheEntry<BookEntitySurrogate>>.NotFound($"Book with id: {id} not found.");
    }
    return ClusterCacheEntry.CreateResult(value, options);
  }
}
