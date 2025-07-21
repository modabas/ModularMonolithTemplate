using ModResults;

namespace ModularMonolith.Modules.FirstService.Features.Books.Orleans;

internal interface IBookGrain : IGrainWithGuidKey
{
  Task<Result<BookEntitySurrogate>> GetBookAsync(CancellationToken ct);
  Task<Result> DeleteBookAsync(CancellationToken ct);
  Task<Result<Guid>> CreateBookAsync(BookEntitySurrogate book, CancellationToken ct);
  Task<Result<BookEntitySurrogate>> UpdateBookAsync(BookEntitySurrogate book, CancellationToken ct);
  Task<Result> InvalidateStateAsync(CancellationToken ct);
}
