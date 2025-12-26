using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using ModEndpoints.Core;

namespace ModularMonolith.Modules.FirstService.Features;

internal class FeaturesRouteGroup : RouteGroupConfigurator
{
  protected override void Configure(
    RouteGroupConfigurationBuilder builder,
    RouteGroupConfigurationContext configurationContext)
  {
    var groupBuilder = builder.MapGroup("/first_service/api/v{version:apiVersion}");
    var apiVersionSet = groupBuilder.NewApiVersionSet()
      .HasApiVersion(new ApiVersion(1))
      .HasApiVersion(new ApiVersion(2))
      .ReportApiVersions()
      .Build();
    groupBuilder.WithApiVersionSet(apiVersionSet);
  }
}
