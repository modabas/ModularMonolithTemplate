using FluentValidation;
using Microsoft.AspNetCore.Http;
using ModEndpoints;
using ModEndpoints.Core;
using ModResults;
using ModularMonolith.Modules.FirstService.FeatureContracts.Features;
using ModularMonolith.Modules.FirstService.Features.Books.Configuration;
using ModularMonolith.Modules.FirstService.Features.Books.Orleans;
using ModularMonolith.Shared.Orleans;

namespace ModularMonolith.Modules.FirstService.Features.Books;

internal record GetBookByIdRequest(Guid Id);

internal class GetBookByIdRequestValidator : AbstractValidator<GetBookByIdRequest>
{
  public GetBookByIdRequestValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
  }
}

[MapToGroup(typeof(BooksV1RouteGroup))]
internal class GetBookById(IGrainFactory grainFactory)
  : WebResultEndpoint<GetBookByIdRequest, GetBookByIdResponse>
{
  private const string Pattern = "/{Id}";

  protected override void Configure(
    IServiceProvider serviceProvider,
    IRouteGroupConfigurator? parentRouteGroup)
  {
    MapGet(Pattern)
      .Produces<GetBookByIdResponse>();
  }

  protected override async Task<Result<GetBookByIdResponse>> HandleAsync(
    GetBookByIdRequest req,
    CancellationToken ct)
  {
    using (var gcts = new GrainCancellationTokenSource())
    {
      using (gcts.Link(ct))
      {
        var result = await grainFactory.GetGrain<IBookGrain>(req.Id).GetBookAsync(gcts.Token);
        return result.ToResult(
          book => new GetBookByIdResponse(
            Id: req.Id,
            Title: book.Title,
            Author: book.Author,
            Price: book.Price));
      }
    }
  }
}
