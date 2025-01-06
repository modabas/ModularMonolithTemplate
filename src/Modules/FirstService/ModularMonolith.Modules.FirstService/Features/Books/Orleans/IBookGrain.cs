using ModResults;

namespace ModularMonolith.Modules.FirstService.Features.Books.Orleans;

internal interface IBookGrain : IGrainWithGuidKey
{
  Task<Result<BookGrainState>> GetBookAsync(GrainCancellationToken gct);
  Task<Result> DeleteBookAsync(GrainCancellationToken gct);
  Task<Result<Guid>> CreateBookAsync(BookGrainState book, GrainCancellationToken gct);
  Task<Result<BookGrainState>> UpdateBookAsync(BookGrainState book, GrainCancellationToken gct);
  Task<Result> InvalidateStateAsync(GrainCancellationToken gct);
}
