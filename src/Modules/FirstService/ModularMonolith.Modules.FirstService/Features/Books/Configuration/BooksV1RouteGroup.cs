using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ModEndpoints.Core;

namespace ModularMonolith.Modules.FirstService.Features.Books.Configuration;

[RouteGroupMember(typeof(FeaturesRouteGroup))]
internal class BooksV1RouteGroup : RouteGroupConfigurator
{
  protected override void Configure(
    IServiceProvider serviceProvider,
    IRouteGroupConfigurator? parentRouteGroup)
  {
    MapGroup("/books")
      .MapToApiVersion(1)
      .WithTags("/FirstService/Books");
  }
}
