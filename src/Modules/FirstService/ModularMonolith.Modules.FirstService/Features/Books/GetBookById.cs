using FluentValidation;
using Microsoft.AspNetCore.Http;
using ModEndpoints;
using ModEndpoints.Core;
using ModResults;
using ModularMonolith.Modules.FirstService.FeatureContracts.Features;
using ModularMonolith.Modules.FirstService.Features.Books.Configuration;
using ModularMonolith.Modules.FirstService.Features.Books.Orleans;

namespace ModularMonolith.Modules.FirstService.Features.Books;

internal record GetBookByIdRequest(Guid Id);

internal class GetBookByIdRequestValidator : AbstractValidator<GetBookByIdRequest>
{
  public GetBookByIdRequestValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
  }
}

[MapToGroup<BooksV1RouteGroup>()]
internal class GetBookById(IGrainFactory grainFactory)
  : WebResultEndpoint<GetBookByIdRequest, GetBookByIdResponse>
{
  private const string Pattern = "/{Id}";

  protected override void Configure(
    EndpointConfigurationBuilder builder,
    EndpointConfigurationContext configurationContext)
  {
    builder.MapGet(Pattern)
      .Produces<GetBookByIdResponse>();
  }

  protected override async Task<WebResult<GetBookByIdResponse>> HandleAsync(
    GetBookByIdRequest req,
    CancellationToken ct)
  {
    var result = await grainFactory.GetGrain<IBookGrain>(req.Id).GetBookAsync(ct);
    return result.ToResult(
      book => new GetBookByIdResponse(
        Id: req.Id,
        Title: book.Title,
        Author: book.Author,
        Price: book.Price));
  }
}
