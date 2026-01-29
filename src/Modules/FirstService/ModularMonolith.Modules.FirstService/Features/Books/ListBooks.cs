using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ModEndpoints;
using ModEndpoints.Core;
using ModularMonolith.Modules.FirstService.Data;
using ModularMonolith.Modules.FirstService.FeatureContracts.Features;
using ModularMonolith.Modules.FirstService.Features.Books.Configuration;

namespace ModularMonolith.Modules.FirstService.Features.Books;

[MapToGroup<BooksV1RouteGroup>()]
internal class ListBooks(FirstServiceDbContext db)
  : WebResultEndpointWithEmptyRequest<ListBooksResponse>
{
  private const string Pattern = "/";

  protected override void Configure(
    EndpointConfigurationBuilder builder,
    EndpointConfigurationContext configurationContext)
  {
    builder.MapGet(Pattern)
      .Produces<ListBooksResponse>();
  }

  protected override async Task<WebResult<ListBooksResponse>> HandleAsync(
    CancellationToken ct)
  {
    var books = await db.Books
      .Select(b => new ListBooksResponseItem(
        b.Id,
        b.Title,
        b.Author,
        b.Price))
      .ToListAsync(ct);

    return new ListBooksResponse(Books: books);
  }
}
