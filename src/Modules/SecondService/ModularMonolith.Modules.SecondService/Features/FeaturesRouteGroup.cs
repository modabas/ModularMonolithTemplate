using ModEndpoints.Core;

namespace ModularMonolith.Modules.SecondService.Features;

internal class FeaturesRouteGroup : RouteGroupConfigurator
{
  protected override void Configure(
    IServiceProvider serviceProvider,
    IRouteGroupConfigurator? parentRouteGroup)
  {
    MapGroup("/second_service/api");
  }
}
