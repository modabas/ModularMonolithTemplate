using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ModEndpoints;
using ModEndpoints.Core;
using ModResults;
using ModularMonolith.Modules.SecondService.Features.Stores.Configuration;
using ModularMonolith.Modules.SecondService.Features.Stores.Orleans;

namespace ModularMonolith.Modules.SecondService.Features.Stores;

internal record DeleteStoreRequest(Guid Id);

internal class DeleteStoreRequestValidator : AbstractValidator<DeleteStoreRequest>
{
  public DeleteStoreRequestValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
  }
}

[MapToGroup<StoresRouteGroup>()]
internal class DeleteStore(IGrainFactory grainFactory)
  : MinimalEndpoint<DeleteStoreRequest, Results<NoContent, ValidationProblem, ProblemHttpResult>>
{
  private const string Pattern = "/{Id}";

  protected override void Configure(
    EndpointConfigurationBuilder builder,
    EndpointConfigurationContext configurationContext)
  {
    builder.MapDelete(Pattern);
  }

  protected override async Task<Results<NoContent, ValidationProblem, ProblemHttpResult>> HandleAsync(
    DeleteStoreRequest req,
    CancellationToken ct)
  {
    var result = await grainFactory.GetGrain<IStoreGrain>(req.Id.ToString()).RemoveAndDeleteAsync(ct);
    return result.Map<Results<NoContent, ValidationProblem, ProblemHttpResult>>(_ => TypedResults.NoContent(), r => TypedResults.Problem(r));
  }
}
