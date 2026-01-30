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
    EndpointConfigurationBuilder builder,
    EndpointConfigurationContext configurationContext)
  {
    builder.MapPut(Pattern)
      .Produces<UpdateBookResponse>();
  }

  protected override async Task<WebResult<UpdateBookResponse>> HandleAsync(
    UpdateBookRequest req,
    CancellationToken ct)
  {
    var getResult = await grainFactory.GetGrain<IBookGrain>(req.Id.ToString()).GetOrCreateAsync(ct);
    if (getResult.IsFailed)
    {
      return Result<UpdateBookResponse>.Fail(getResult);
    }

    var book = new BookEntitySurrogate(
      Title: req.Body.Title,
      Author: req.Body.Author,
      Price: req.Body.Price);

    var result = await grainFactory.GetGrain<IBookGrain>(req.Id.ToString()).SetAndWriteAsync(book, ct);
    return result.ToResult(
      book => new UpdateBookResponse(
        Id: req.Id,
        Title: book.Title,
        Author: book.Author,
        Price: book.Price));
  }
}
