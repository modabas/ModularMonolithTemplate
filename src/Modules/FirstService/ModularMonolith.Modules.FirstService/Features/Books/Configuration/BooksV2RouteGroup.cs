using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ModEndpoints.Core;

namespace ModularMonolith.Modules.FirstService.Features.Books.Configuration;

[RouteGroupMember(typeof(FeaturesRouteGroup))]
internal class BooksV2RouteGroup : RouteGroupConfigurator
{
  protected override void Configure(
    IServiceProvider serviceProvider,
    IRouteGroupConfigurator? parentRouteGroup)
  {
    MapGroup("/books")
      .MapToApiVersion(2)
      .WithTags("/FirstService/Books");
  }
}
