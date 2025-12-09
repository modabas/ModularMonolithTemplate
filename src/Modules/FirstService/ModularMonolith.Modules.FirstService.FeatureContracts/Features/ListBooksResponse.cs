namespace ModularMonolith.Modules.FirstService.FeatureContracts.Features;

public record ListBooksResponse(List<ListBooksResponseItem> Books);
public record ListBooksResponseItem(Guid Id, string Title, string Author, decimal Price);
