using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ModEndpoints.Core;

namespace ModularMonolith.Modules.FirstService.Features.Books.Configuration;

[MapToGroup<FeaturesRouteGroup>()]
internal class BooksV2RouteGroup : RouteGroupConfigurator
{
  protected override void Configure(
    RouteGroupConfigurationBuilder builder,
    RouteGroupConfigurationContext configurationContext)
  {
    builder.MapGroup("/books")
      .MapToApiVersion(2)
      .WithTags("/FirstService/Books");
  }
}
