using FluentValidation;
using ModEndpoints;
using ModEndpoints.Core;
using ModResults;
using ModularMonolith.Modules.SecondService.Features.Stores.Configuration;
using ModularMonolith.Modules.SecondService.Features.Stores.Orleans;
using ModularMonolith.Shared.Orleans;

namespace ModularMonolith.Modules.SecondService.Features.Stores;

internal record DeleteStoreRequest(Guid Id);

internal class DeleteStoreRequestValidator : AbstractValidator<DeleteStoreRequest>
{
  public DeleteStoreRequestValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
  }
}

[MapToGroup<StoresRouteGroup>()]
internal class DeleteStore(IGrainFactory grainFactory)
  : BusinessResultEndpoint<DeleteStoreRequest>
{
  private const string Pattern = "/{Id}";

  protected override void Configure(
    EndpointConfigurationBuilder builder,
    ConfigurationContext<EndpointConfigurationParameters> configurationContext)
  {
    builder.MapDelete(Pattern);
  }

  protected override async Task<Result> HandleAsync(
    DeleteStoreRequest req,
    CancellationToken ct)
  {
    var result = await grainFactory.GetGrain<IStoreGrain>(req.Id).DeleteStoreAsync(ct);
    return result;
  }
}
