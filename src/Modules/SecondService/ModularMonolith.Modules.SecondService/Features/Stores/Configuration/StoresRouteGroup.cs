using Microsoft.AspNetCore.Http;
using ModEndpoints.Core;

namespace ModularMonolith.Modules.SecondService.Features.Stores.Configuration;

[MapToGroup(typeof(FeaturesRouteGroup))]
internal class StoresRouteGroup : RouteGroupConfigurator
{
  protected override void Configure(
    IServiceProvider serviceProvider,
    IRouteGroupConfigurator? parentRouteGroup)
  {
    MapGroup("/stores")
      .WithTags("/SecondService/Stores");
  }
}
