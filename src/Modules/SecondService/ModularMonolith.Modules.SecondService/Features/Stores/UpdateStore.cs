using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ModEndpoints;
using ModEndpoints.Core;
using ModResults;
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
  : BusinessResultEndpoint<UpdateStoreRequest, UpdateStoreResponse>
{
  private const string Pattern = "/{Id}";

  protected override void Configure(
    EndpointConfigurationBuilder builder,
    EndpointConfigurationContext configurationContext)
  {
    builder.MapPut(Pattern);
  }

  protected override async Task<Result<UpdateStoreResponse>> HandleAsync(
    UpdateStoreRequest req,
    CancellationToken ct)
  {
    var store = new StoreEntitySurrogate(
      Name: req.Body.Name);

    var result = await grainFactory.GetGrain<IStoreGrain>(req.Id).UpdateStoreAsync(store, ct);
    return result.ToResult(
      book => new UpdateStoreResponse(
        Id: req.Id,
        Name: book.Name));
  }
}
