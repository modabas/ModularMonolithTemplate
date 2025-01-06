namespace ModularMonolith.Modules.FirstService.FeatureContracts.Features;

public record CreateBookRequestBody(string Title, string Author, decimal Price);
