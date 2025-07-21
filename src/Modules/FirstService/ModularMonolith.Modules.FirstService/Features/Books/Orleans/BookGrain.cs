using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModResults;
using ModularMonolith.Modules.FirstService.Data;
using ModularMonolith.Shared.Data.SimpleOutbox.Extensions;
using ModularMonolith.Shared.IntegrationContracts.FirstService.Books;
using ModularMonolith.Shared.Orleans;

namespace ModularMonolith.Modules.FirstService.Features.Books.Orleans;

internal class BookGrain : BaseGrain, IBookGrain
{
  private BookEntitySurrogate? _cache;

  public async Task<Result<Guid>> CreateBookAsync(BookEntitySurrogate book, CancellationToken ct)
  {
    var id = this.GetPrimaryKey();
    var db = RequestServices.GetRequiredService<FirstServiceDbContext>();

    db.Books.Add(book.ToEntity(id));
    db.AddToOutbox(new BookCreatedEvent(id, book.Title, book.Author, book.Price));
    await db.SaveChangesAsync(ct);

    //set in-memory
    _cache = book;

    return id;
  }

  public async Task<Result> DeleteBookAsync(CancellationToken ct)
  {
    var id = this.GetPrimaryKey();
    var db = RequestServices.GetRequiredService<FirstServiceDbContext>();

    var entity = await db.Books.FirstOrDefaultAsync(b => b.Id == id, ct);
    if (entity is null)
    {
      //set in-memory
      _cache = null;
      return Result.NotFound();
    }
    db.Remove(entity);
    db.AddToOutbox(new BookDeletedEvent(id));
    await db.SaveChangesAsync(ct);

    //set in-memory
    _cache = null;

    return Result.Ok();
  }

  public async Task<Result<BookEntitySurrogate>> GetBookAsync(CancellationToken ct)
  {
    var id = this.GetPrimaryKey();
    var db = RequestServices.GetRequiredService<FirstServiceDbContext>();

    //check in-memory
    if (_cache is null)
    {
      var entity = await db.Books.FirstOrDefaultAsync(b => b.Id == id, ct);

      //set in-memory
      _cache = entity?.ToSurrogate();
    }

    var result = _cache is null ?
      Result<BookEntitySurrogate>.NotFound(string.Format("Book with id: {0} not found.", id)) :
      _cache;
    return result;
  }

  public Task<Result> InvalidateStateAsync(CancellationToken ct)
  {
    _cache = null;
    return Task.FromResult(Result.Ok());
  }

  public async Task<Result<BookEntitySurrogate>> UpdateBookAsync(BookEntitySurrogate book, CancellationToken ct)
  {
    var id = this.GetPrimaryKey();
    var db = RequestServices.GetRequiredService<FirstServiceDbContext>();

    var updated = await db.Books
      .Where(b => b.Id == id)
      .ExecuteUpdateAsync(setters => setters
        .SetProperty(b => b.Title, book.Title)
        .SetProperty(b => b.Author, book.Author)
        .SetProperty(b => b.Price, book.Price),
      ct);

    if (updated > 0)
    {
      //set in-memory
      _cache = book;
      return book;
    }
    else
    {
      return Result<BookEntitySurrogate>.NotFound();
    }
  }
}
