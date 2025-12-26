using ModEndpoints.Core;

namespace ModularMonolith.Modules.SecondService.Features;

internal class FeaturesRouteGroup : RouteGroupConfigurator
{
  protected override void Configure(
    RouteGroupConfigurationBuilder builder,
    RouteGroupConfigurationContext configurationContext)
  {
    builder.MapGroup("/second_service/api");
  }
}
