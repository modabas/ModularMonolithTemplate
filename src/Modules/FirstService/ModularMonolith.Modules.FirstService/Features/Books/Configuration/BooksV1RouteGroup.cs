using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ModEndpoints.Core;

namespace ModularMonolith.Modules.FirstService.Features.Books.Configuration;

[MapToGroup<FeaturesRouteGroup>()]
internal class BooksV1RouteGroup : RouteGroupConfigurator
{
  protected override void Configure(
    RouteGroupConfigurationBuilder builder,
    ConfigurationContext<RouteGroupConfigurationParameters> configurationContext)
  {
    builder.MapGroup("/books")
      .MapToApiVersion(1)
      .WithTags("/FirstService/Books");
  }
}
