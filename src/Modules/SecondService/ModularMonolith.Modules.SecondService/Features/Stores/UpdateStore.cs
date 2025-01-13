using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ModEndpoints;
using ModEndpoints.Core;
using ModResults;
using ModularMonolith.Modules.SecondService.FeatureContracts.Features.Stores;
using ModularMonolith.Modules.SecondService.Features.Stores.Configuration;
using ModularMonolith.Modules.SecondService.Features.Stores.Orleans;
using ModularMonolith.Shared.Orleans;

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

[MapToGroup(typeof(StoresRouteGroup))]
internal class UpdateStore(IGrainFactory grainFactory)
  : BusinessResultEndpoint<UpdateStoreRequest, UpdateStoreResponse>
{
  private const string Pattern = "/{Id}";

  protected override void Configure(
    IServiceProvider serviceProvider,
    IRouteGroupConfigurator? parentRouteGroup)
  {
    MapPut(Pattern);
  }

  protected override async Task<Result<UpdateStoreResponse>> HandleAsync(
    UpdateStoreRequest req,
    CancellationToken ct)
  {
    var store = new StoreEntitySurrogate(
      Name: req.Body.Name);

    using (var gcts = new GrainCancellationTokenSource())
    {
      using (gcts.Link(ct))
      {
        var result = await grainFactory.GetGrain<IStoreGrain>(req.Id).UpdateStoreAsync(store, gcts.Token);
        return result.ToResult(
          book => new UpdateStoreResponse(
            Id: req.Id,
            Name: book.Name));
      }
    }
  }
}
