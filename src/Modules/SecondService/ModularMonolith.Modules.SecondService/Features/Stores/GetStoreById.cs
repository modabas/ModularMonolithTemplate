using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ModEndpoints.Core;
using ModResults.MinimalApis;
using ModularMonolith.Modules.SecondService.FeatureContracts.Features.Stores;
using ModularMonolith.Modules.SecondService.Features.Stores.Configuration;
using ModularMonolith.Modules.SecondService.Features.Stores.Orleans;

namespace ModularMonolith.Modules.SecondService.Features.Stores;

internal record GetStoreByIdRequest(Guid Id);

internal class GetStoreByIdRequestValidator : AbstractValidator<GetStoreByIdRequest>
{
  public GetStoreByIdRequestValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
  }
}

[MapToGroup<StoresRouteGroup>()]
internal class GetStoreById(IGrainFactory grainFactory)
  : MinimalEndpoint<GetStoreByIdRequest, Results<Ok<GetStoreByIdResponse>, ValidationProblem, NotFound, ProblemHttpResult>>
{
  private const string Pattern = "/{Id}";

  protected override void Configure(
    EndpointConfigurationBuilder builder,
    EndpointConfigurationContext configurationContext)
  {
    builder.MapGet(Pattern);
  }

  protected override async Task<Results<Ok<GetStoreByIdResponse>, ValidationProblem, NotFound, ProblemHttpResult>> HandleAsync(
    GetStoreByIdRequest req,
    CancellationToken ct)
  {
    var result = await grainFactory.GetGrain<IStoreGrain>(req.Id.ToString()).GetOrCreateAsync(ct);
    if (result.IsOk)
    {
      return TypedResults.Ok(new GetStoreByIdResponse(
        Id: req.Id,
        Name: result.Value.Name));
    }
    return TypedResults.Problem(result);
  }
}

