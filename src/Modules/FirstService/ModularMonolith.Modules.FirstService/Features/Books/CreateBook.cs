using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModEndpoints;
using ModEndpoints.Core;
using ModResults;
using ModularMonolith.Modules.FirstService.FeatureContracts.Features;
using ModularMonolith.Modules.FirstService.Features.Books.Configuration;
using ModularMonolith.Modules.FirstService.Features.Books.Orleans;
using ModularMonolith.Shared.Guids;

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
internal class CreateBook(IGrainFactory grainFactory)
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

    var result = await grainFactory.GetGrain<IBookGrain>(id).CreateBookAsync(book, ct);
    return WebResults.WithLocationRouteOnSuccess(
      result.ToResult(v => new CreateBookResponse(v)),
      typeof(GetBookById).FullName,
      new { id = id });
  }
}
