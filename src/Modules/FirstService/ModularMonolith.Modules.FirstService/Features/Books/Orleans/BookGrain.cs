using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModResults;
using ModularMonolith.Modules.FirstService.Data;
using ModularMonolith.Modules.FirstService.IntegrationContracts.Integrations.Books;
using ModularMonolith.Shared.Data.SimpleOutbox.Extensions;
using ModularMonolith.Shared.Orleans;

namespace ModularMonolith.Modules.FirstService.Features.Books.Orleans;

internal class BookGrain : BaseGrain, IBookGrain
{
  private BookGrainState? _state;

  public async Task<Result<Guid>> CreateBookAsync(BookGrainState book, GrainCancellationToken gct)
  {
    var id = this.GetPrimaryKey();
    var ct = gct.CancellationToken;
    var db = RequestServices.GetRequiredService<FirstServiceDbContext>();

    db.Books.Add(book.ToEntity(id));
    db.AddToOutbox(new BookCreatedEvent(id, book.Title, book.Author, book.Price));
    await db.SaveChangesAsync(ct);

    //set in-memory
    _state = book;

    return id;
  }

  public async Task<Result> DeleteBookAsync(GrainCancellationToken gct)
  {
    var id = this.GetPrimaryKey();
    var ct = gct.CancellationToken;
    var db = RequestServices.GetRequiredService<FirstServiceDbContext>();

    var deleted = await db.Books.Where(b => b.Id == id).ExecuteDeleteAsync(ct);

    //set in-memory
    _state = null;

    return deleted > 0 ? Result.Ok() : Result.NotFound();
  }

  public async Task<Result<BookGrainState>> GetBookAsync(GrainCancellationToken gct)
  {
    var id = this.GetPrimaryKey();
    var ct = gct.CancellationToken;
    var db = RequestServices.GetRequiredService<FirstServiceDbContext>();

    //check in-memory
    if (_state is null)
    {
      var entity = await db.Books.FirstOrDefaultAsync(b => b.Id == id, ct);

      //set in-memory
      _state = entity?.ToState();
    }

    //create event
    db.AddToOutbox(new BookQueriedByIdEvent(id));
    await db.SaveChangesAsync(ct);

    var result = _state is null ?
      Result<BookGrainState>.NotFound(string.Format("Book with id: {0} not found.", id)) :
      _state;
    return result;
  }

  public Task<Result> InvalidateStateAsync(GrainCancellationToken gct)
  {
    _state = null;
    return Task.FromResult(Result.Ok());
  }

  public async Task<Result<BookGrainState>> UpdateBookAsync(BookGrainState book, GrainCancellationToken gct)
  {
    var id = this.GetPrimaryKey();
    var ct = gct.CancellationToken;
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
      _state = book;
      return book;
    }
    else
    {
      return Result<BookGrainState>.NotFound();
    }
  }
}
