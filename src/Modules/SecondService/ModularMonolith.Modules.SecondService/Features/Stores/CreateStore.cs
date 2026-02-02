using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ModEndpoints;
using ModEndpoints.Core;
using ModResults;
using ModularMonolith.Modules.SecondService.Data;
using ModularMonolith.Modules.SecondService.FeatureContracts.Features.Stores;
using ModularMonolith.Modules.SecondService.Features.Stores.Configuration;
using ModularMonolith.Modules.SecondService.Features.Stores.Orleans;
using ModularMonolith.Shared.Data.SimpleOutbox.Extensions;
using ModularMonolith.Shared.Guids;
using ModularMonolith.Shared.IntegrationContracts.SecondService.Stores;

namespace ModularMonolith.Modules.SecondService.Features.Stores;

internal record CreateStoreRequest([FromBody] CreateStoreRequestBody Body);

internal class CreateStoreRequestValidator : AbstractValidator<CreateStoreRequest>
{
  public CreateStoreRequestValidator()
  {
    RuleFor(x => x.Body.Name).NotEmpty();
  }
}

[MapToGroup<StoresRouteGroup>()]
internal class CreateStore(SecondServiceDbContext db)
  : BusinessResultEndpoint<CreateStoreRequest, CreateStoreResponse>
{
  private const string Pattern = "/";

  protected override void Configure(
    EndpointConfigurationBuilder builder,
    EndpointConfigurationContext configurationContext)
  {
    builder.MapPost(Pattern);
  }

  protected override async Task<Result<CreateStoreResponse>> HandleAsync(
    CreateStoreRequest req,
    CancellationToken ct)
  {
    var store = new StoreEntitySurrogate(
      Name: req.Body.Name);
    var id = GuidV7.CreateVersion7();

    db.Stores.Add(store.ToEntity(id));
    db.AddToOutbox(new StoreCreatedEvent(id, store.Name));
    await db.SaveChangesAsync(ct);

    return new CreateStoreResponse(id);
  }
}

