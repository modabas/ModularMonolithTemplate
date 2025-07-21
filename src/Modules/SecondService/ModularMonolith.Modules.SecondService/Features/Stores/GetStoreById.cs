using FluentValidation;
using ModEndpoints;
using ModEndpoints.Core;
using ModResults;
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
  : BusinessResultEndpoint<GetStoreByIdRequest, GetStoreByIdResponse>
{
  private const string Pattern = "/{Id}";

  protected override void Configure(
    EndpointConfigurationBuilder builder,
    ConfigurationContext<EndpointConfigurationParameters> configurationContext)
  {
    builder.MapGet(Pattern);
  }

  protected override async Task<Result<GetStoreByIdResponse>> HandleAsync(
    GetStoreByIdRequest req,
    CancellationToken ct)
  {
    var result = await grainFactory.GetGrain<IStoreGrain>(req.Id).GetStoreAsync(ct);
    return result.ToResult(
      book => new GetStoreByIdResponse(
        Id: req.Id,
        Name: book.Name));
  }
}

