using Microsoft.AspNetCore.Http;
using ModEndpoints.Core;

namespace ModularMonolith.Modules.SecondService.Features.Stores.Configuration;

[MapToGroup<FeaturesRouteGroup>()]
internal class StoresRouteGroup : RouteGroupConfigurator
{
  protected override void Configure(
    RouteGroupConfigurationBuilder builder,
    ConfigurationContext<RouteGroupConfigurationParameters> configurationContext)
  {
    builder.MapGroup("/stores")
      .WithTags("/SecondService/Stores");
  }
}
