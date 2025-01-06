using ModularMonolith.Modules.FirstService.Features.Books.Data;

namespace ModularMonolith.Modules.FirstService.Features.Books.Orleans;
internal static class BookExtensions
{
  public static BookEntity ToEntity(this BookGrainState state, Guid id)
  {
    return new BookEntity()
    {
      Id = id,
      Author = state.Author,
      Price = state.Price,
      Title = state.Title
    };
  }

  public static BookGrainState ToState(this BookEntity entity)
  {
    return new BookGrainState(
       Title: entity.Title,
       Author: entity.Author,
       Price: entity.Price);
  }
}
