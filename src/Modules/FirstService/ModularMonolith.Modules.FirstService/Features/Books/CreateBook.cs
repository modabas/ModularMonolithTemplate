using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModEndpoints;
using ModEndpoints.Core;
using ModularMonolith.Modules.FirstService.Data;
using ModularMonolith.Modules.FirstService.FeatureContracts.Features;
using ModularMonolith.Modules.FirstService.Features.Books.Configuration;
using ModularMonolith.Modules.FirstService.Features.Books.Orleans;
using ModularMonolith.Shared.Data.SimpleOutbox.Extensions;
using ModularMonolith.Shared.Guids;
using ModularMonolith.Shared.IntegrationContracts.FirstService.Books;

namespace ModularMonolith.Modules.FirstService.Features.Books;

internal record CreateBookRequest([FromBody] CreateBookRequestBody Body);

internal class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
{
  public CreateBookRequestValidator()
  {
    RuleFor(x => x.Body.Title).NotEmpty();
    RuleFor(x => x.Body.Author).NotEmpty();
    RuleFor(x => x.Body.Price).GreaterThan(0);
  }
}

[MapToGroup<BooksV1RouteGroup>()]
internal class CreateBook(
  FirstServiceDbContext db)
  : WebResultEndpoint<CreateBookRequest, CreateBookResponse>
{
  private const string Pattern = "/";

  protected override void Configure(
    EndpointConfigurationBuilder builder,
    EndpointConfigurationContext configurationContext)
  {
    builder.MapPost(Pattern)
      .Produces<CreateBookResponse>(StatusCodes.Status201Created);
  }
  protected override async Task<WebResult<CreateBookResponse>> HandleAsync(
    CreateBookRequest req,
    CancellationToken ct)
  {
    var book = new BookEntitySurrogate(
      Title: req.Body.Title,
      Author: req.Body.Author,
      Price: req.Body.Price);

    var id = GuidV7.CreateVersion7();

    db.Books.Add(book.ToEntity(id));
    db.AddToOutbox(new BookCreatedEvent(id, book.Title, book.Author, book.Price));
    await db.SaveChangesAsync(ct);

    return WebResults.WithLocationRouteOnSuccess(
      new CreateBookResponse(id),
      typeof(GetBookById).FullName,
      new { id = id });
  }
}
