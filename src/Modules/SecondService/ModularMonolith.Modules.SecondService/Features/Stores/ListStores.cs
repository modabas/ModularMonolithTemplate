using Microsoft.EntityFrameworkCore;
using ModEndpoints;
using ModEndpoints.Core;
using ModResults;
using ModularMonolith.Modules.SecondService.Data;
using ModularMonolith.Modules.SecondService.FeatureContracts.Features.Stores;
using ModularMonolith.Modules.SecondService.Features.Stores.Configuration;

namespace ModularMonolith.Modules.SecondService.Features.Stores;

[RouteGroupMember(typeof(StoresRouteGroup))]
internal class ListStores(SecondServiceDbContext db)
  : BusinessResultEndpointWithEmptyRequest<ListStoresResponse>
{
  private const string Pattern = "/";

  protected override void Configure(
    IServiceProvider serviceProvider,
    IRouteGroupConfigurator? parentRouteGroup)
  {
    MapGet(Pattern);
  }

  protected override async Task<Result<ListStoresResponse>> HandleAsync(
    CancellationToken ct)
  {
    var stores = await db.Stores
      .Select(b => new ListStoresResponseItem(
        b.Id,
        b.Name))
      .ToListAsync(ct);

    return new ListStoresResponse(Stores: stores);
  }
}
