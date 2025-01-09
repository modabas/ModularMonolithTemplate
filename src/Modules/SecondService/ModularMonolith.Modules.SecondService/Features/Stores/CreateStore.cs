using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ModEndpoints;
using ModEndpoints.Core;
using ModResults;
using ModularMonolith.Modules.SecondService.FeatureContracts.Features.Stores;
using ModularMonolith.Modules.SecondService.Features.Stores.Configuration;
using ModularMonolith.Modules.SecondService.Features.Stores.Orleans;
using ModularMonolith.Shared.Guids;
using ModularMonolith.Shared.Orleans;

namespace ModularMonolith.Modules.SecondService.Features.Stores;

internal record CreateStoreRequest([FromBody] CreateStoreRequestBody Body);

internal class CreateStoreRequestValidator : AbstractValidator<CreateStoreRequest>
{
  public CreateStoreRequestValidator()
  {
    RuleFor(x => x.Body.Name).NotEmpty();
  }
}

[RouteGroupMember(typeof(StoresRouteGroup))]
internal class CreateStore(IGrainFactory grainFactory)
  : BusinessResultEndpoint<CreateStoreRequest, CreateStoreResponse>
{
  private const string Pattern = "/";

  protected override void Configure(
    IServiceProvider serviceProvider,
    IRouteGroupConfigurator? parentRouteGroup)
  {
    MapPost(Pattern);
  }

  protected override async Task<Result<CreateStoreResponse>> HandleAsync(
    CreateStoreRequest req,
    CancellationToken ct)
  {
    var store = new StoreEntitySurrogate(
      Name: req.Body.Name);
    var id = GuidV7.CreateVersion7();

    using (var gcts = new GrainCancellationTokenSource())
    {
      using (gcts.Link(ct))
      {
        var result = await grainFactory.GetGrain<IStoreGrain>(id).CreateStoreAsync(store, gcts.Token);
        return result.ToResult(id => new CreateStoreResponse(id));
      }
    }
  }
}

