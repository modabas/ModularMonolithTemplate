using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ModEndpoints;
using ModEndpoints.Core;
using ModularMonolith.Modules.SecondService.FeatureContracts.Features.Stores;
using ModularMonolith.Modules.SecondService.Features.Stores.Configuration;
using ModularMonolith.Modules.SecondService.Features.Stores.Orleans;

namespace ModularMonolith.Modules.SecondService.Features.Stores;

internal record UpdateStoreRequest(Guid Id, [FromBody] UpdateStoreRequestBody Body);

internal class UpdateStoreRequestValidator : AbstractValidator<UpdateStoreRequest>
{
  public UpdateStoreRequestValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
    RuleFor(x => x.Body.Name).NotEmpty();
  }
}

[MapToGroup<StoresRouteGroup>()]
internal class UpdateStore(IGrainFactory grainFactory)
  : MinimalEndpoint<UpdateStoreRequest, Results<Ok<UpdateStoreResponse>, ValidationProblem, NotFound, ProblemHttpResult>>
{
  private const string Pattern = "/{Id}";

  protected override void Configure(
    EndpointConfigurationBuilder builder,
    EndpointConfigurationContext configurationContext)
  {
    builder.MapPut(Pattern);
  }

  protected override async Task<Results<Ok<UpdateStoreResponse>, ValidationProblem, NotFound, ProblemHttpResult>> HandleAsync(
    UpdateStoreRequest req,
    CancellationToken ct)
  {
    var getResult = await grainFactory.GetGrain<IStoreGrain>(req.Id.ToString()).GetOrCreateAsync(ct);
    if (getResult.IsFailed)
    {
      return TypedResults.Problem(getResult);
    }

    var store = new StoreEntitySurrogate(
      Name: req.Body.Name);

    var result = await grainFactory.GetGrain<IStoreGrain>(req.Id.ToString()).SetAndWriteAsync(store, ct);
    if (result.IsOk)
    {
      return TypedResults.Ok(new UpdateStoreResponse(
        Id: req.Id,
        Name: result.Value.Name));
    }
    return TypedResults.Problem(result);
  }
}
