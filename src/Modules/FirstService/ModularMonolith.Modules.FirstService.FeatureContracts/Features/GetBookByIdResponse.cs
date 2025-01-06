namespace ModularMonolith.Modules.FirstService.FeatureContracts.Features;

public record GetBookByIdResponse(Guid Id, string Title, string Author, decimal Price);
