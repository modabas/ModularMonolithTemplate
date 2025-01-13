using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModEndpoints;
using ModEndpoints.Core;
using ModResults;
using ModularMonolith.Modules.FirstService.FeatureContracts.Features;
using ModularMonolith.Modules.FirstService.Features.Books.Configuration;
using ModularMonolith.Modules.FirstService.Features.Books.Orleans;
using ModularMonolith.Shared.Orleans;

namespace ModularMonolith.Modules.FirstService.Features.Books;

internal record UpdateBookRequest(Guid Id, [FromBody] UpdateBookRequestBody Body);

internal class UpdateBookRequestValidator : AbstractValidator<UpdateBookRequest>
{
  public UpdateBookRequestValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
    RuleFor(x => x.Body.Title).NotEmpty();
    RuleFor(x => x.Body.Author).NotEmpty();
    RuleFor(x => x.Body.Price).GreaterThan(0);
  }
}

[MapToGroup<BooksV1RouteGroup>()]
internal class UpdateBook(IGrainFactory grainFactory)
  : WebResultEndpoint<UpdateBookRequest, UpdateBookResponse>
{
  private const string Pattern = "/{Id}";

  protected override void Configure(
    IServiceProvider serviceProvider,
    IRouteGroupConfigurator? parentRouteGroup)
  {
    MapPut(Pattern)
      .Produces<UpdateBookResponse>();
  }

  protected override async Task<Result<UpdateBookResponse>> HandleAsync(
    UpdateBookRequest req,
    CancellationToken ct)
  {
    var book = new BookEntitySurrogate(
      Title: req.Body.Title,
      Author: req.Body.Author,
      Price: req.Body.Price);

    using (var gcts = new GrainCancellationTokenSource())
    {
      using (gcts.Link(ct))
      {
        var result = await grainFactory.GetGrain<IBookGrain>(req.Id).UpdateBookAsync(book, gcts.Token);
        return result.ToResult(
          book => new UpdateBookResponse(
            Id: req.Id,
            Title: book.Title,
            Author: book.Author,
            Price: book.Price));
      }
    }
  }
}
