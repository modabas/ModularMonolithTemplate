namespace ModularMonolith.Modules.FirstService.FeatureContracts.Features;

public record UpdateBookRequestBody(string Title, string Author, decimal Price);
