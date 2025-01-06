namespace ModularMonolith.Modules.FirstService.FeatureContracts.Features;

public record UpdateBookResponse(Guid Id, string Title, string Author, decimal Price);

