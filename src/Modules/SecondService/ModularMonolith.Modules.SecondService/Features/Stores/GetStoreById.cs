using FluentValidation;
using ModEndpoints;
using ModEndpoints.Core;
using ModResults;
using ModularMonolith.Modules.SecondService.FeatureContracts.Features.Stores;
using ModularMonolith.Modules.SecondService.Features.Stores.Configuration;
using ModularMonolith.Modules.SecondService.Features.Stores.Orleans;
using ModularMonolith.Shared.Orleans;

namespace ModularMonolith.Modules.SecondService.Features.Stores;

internal record GetStoreByIdRequest(Guid Id);

internal class GetStoreByIdRequestValidator : AbstractValidator<GetStoreByIdRequest>
{
  public GetStoreByIdRequestValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
  }
}

[RouteGroupMember(typeof(StoresRouteGroup))]
internal class GetStoreById(IGrainFactory grainFactory)
  : BusinessResultEndpoint<GetStoreByIdRequest, GetStoreByIdResponse>
{
  private const string Pattern = "/{Id}";

  protected override void Configure(
    IServiceProvider serviceProvider,
    IRouteGroupConfigurator? parentRouteGroup)
  {
    MapGet(Pattern);
  }

  protected override async Task<Result<GetStoreByIdResponse>> HandleAsync(
    GetStoreByIdRequest req,
    CancellationToken ct)
  {
    using (var gcts = new GrainCancellationTokenSource())
    {
      using (gcts.Link(ct))
      {
        var result = await grainFactory.GetGrain<IStoreGrain>(req.Id).GetStoreAsync(gcts.Token);
        return result.ToResult(
          book => new GetStoreByIdResponse(
            Id: req.Id,
            Name: book.Name));
      }
    }
  }
}

