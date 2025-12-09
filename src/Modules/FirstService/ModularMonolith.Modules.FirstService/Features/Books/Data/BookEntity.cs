using ModularMonolith.Shared.Data;

namespace ModularMonolith.Modules.FirstService.Features.Books.Data;

internal class BookEntity : BaseEntity
{
  public string Title { get; set; } = string.Empty;
  public string Author { get; set; } = string.Empty;
  public decimal Price { get; set; }
}
