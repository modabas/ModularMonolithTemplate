using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ModEndpoints;
using ModEndpoints.Core;
using ModularMonolith.Modules.SecondService.Data;
using ModularMonolith.Modules.SecondService.FeatureContracts.Features.Stores;
using ModularMonolith.Modules.SecondService.Features.Stores.Configuration;

namespace ModularMonolith.Modules.SecondService.Features.Stores;

[MapToGroup<StoresRouteGroup>()]
internal class ListStores(SecondServiceDbContext db)
  : MinimalEndpoint<Results<Ok<ListStoresResponse>, NotFound>>
{
  private const string Pattern = "/";

  protected override void Configure(
    EndpointConfigurationBuilder builder,
    EndpointConfigurationContext configurationContext)
  {
    builder.MapGet(Pattern);
  }

  protected override async Task<Results<Ok<ListStoresResponse>, NotFound>> HandleAsync(
    CancellationToken ct)
  {
    var stores = await db.Stores
      .Select(b => new ListStoresResponseItem(
        b.Id,
        b.Name))
      .ToListAsync(ct);

    if (stores.Count == 0)
    {
      return TypedResults.NotFound();
    }
    return TypedResults.Ok(new ListStoresResponse(Stores: stores));
  }
}
