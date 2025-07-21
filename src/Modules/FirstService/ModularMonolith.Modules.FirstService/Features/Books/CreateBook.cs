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
using ModularMonolith.Shared.Orleans;

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
internal class CreateBook(IGrainFactory grainFactory, ILocationStore location)
  : WebResultEndpoint<CreateBookRequest, CreateBookResponse>
{
  private const string Pattern = "/";

  protected override void Configure(
    EndpointConfigurationBuilder builder,
    ConfigurationContext<EndpointConfigurationParameters> configurationContext)
  {
    builder.MapPost(Pattern)
      .Produces<CreateBookResponse>(StatusCodes.Status201Created);
  }
  protected override async Task<Result<CreateBookResponse>> HandleAsync(
    CreateBookRequest req,
    CancellationToken ct)
  {
    var book = new BookEntitySurrogate(
      Title: req.Body.Title,
      Author: req.Body.Author,
      Price: req.Body.Price);

    var id = GuidV7.CreateVersion7();

    var result = await grainFactory.GetGrain<IBookGrain>(id).CreateBookAsync(book, ct);
    return await result.ToResultAsync(
      async (id, state, ct) =>
      {
        await state.Location.SetValueAsync(
          typeof(GetBookById).FullName,
          new { id = id },
          ct);
        return new CreateBookResponse(id);
      },
      new { Location = location },
      ct);
  }
}
