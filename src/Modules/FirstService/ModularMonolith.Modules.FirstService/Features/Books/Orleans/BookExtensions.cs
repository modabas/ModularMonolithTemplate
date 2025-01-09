using ModularMonolith.Modules.FirstService.Features.Books.Data;

namespace ModularMonolith.Modules.FirstService.Features.Books.Orleans;
internal static class BookExtensions
{
  public static BookEntity ToEntity(this BookEntitySurrogate state, Guid id)
  {
    return new BookEntity()
    {
      Id = id,
      Author = state.Author,
      Price = state.Price,
      Title = state.Title
    };
  }

  public static BookEntitySurrogate ToSurrogate(this BookEntity entity)
  {
    return new BookEntitySurrogate(
       Title: entity.Title,
       Author: entity.Author,
       Price: entity.Price);
  }
}
