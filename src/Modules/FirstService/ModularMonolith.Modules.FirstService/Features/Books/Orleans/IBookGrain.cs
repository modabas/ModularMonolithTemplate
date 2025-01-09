using ModResults;

namespace ModularMonolith.Modules.FirstService.Features.Books.Orleans;

internal interface IBookGrain : IGrainWithGuidKey
{
  Task<Result<BookEntitySurrogate>> GetBookAsync(GrainCancellationToken gct);
  Task<Result> DeleteBookAsync(GrainCancellationToken gct);
  Task<Result<Guid>> CreateBookAsync(BookEntitySurrogate book, GrainCancellationToken gct);
  Task<Result<BookEntitySurrogate>> UpdateBookAsync(BookEntitySurrogate book, GrainCancellationToken gct);
  Task<Result> InvalidateStateAsync(GrainCancellationToken gct);
}
