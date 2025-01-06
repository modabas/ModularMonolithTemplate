using FluentValidation;
using ModEndpoints;
using ModEndpoints.Core;
using ModResults;
using ModularMonolith.Modules.FirstService.Features.Books.Configuration;
using ModularMonolith.Modules.FirstService.Features.Books.Orleans;
using ModularMonolith.Shared.Orleans;

namespace ModularMonolith.Modules.FirstService.Features.Books;

internal record DeleteBookRequest(Guid Id);

internal class DeleteBookRequestValidator : AbstractValidator<DeleteBookRequest>
{
  public DeleteBookRequestValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
  }
}

[RouteGroupMember(typeof(BooksV1RouteGroup))]
internal class DeleteBook(IGrainFactory grainFactory)
  : WebResultEndpoint<DeleteBookRequest>
{
  private const string Pattern = "/{Id}";

  protected override void Configure(
    IServiceProvider serviceProvider,
    IRouteGroupConfigurator? parentRouteGroup)
  {
    MapDelete(Pattern);
  }

  protected override async Task<Result> HandleAsync(
  DeleteBookRequest req,
    CancellationToken ct)
  {
    using (var gcts = new GrainCancellationTokenSource())
    {
      using (gcts.Link(ct))
      {
        var result = await grainFactory.GetGrain<IBookGrain>(req.Id).DeleteBookAsync(gcts.Token);
        return result;
      }
    }
  }
}
