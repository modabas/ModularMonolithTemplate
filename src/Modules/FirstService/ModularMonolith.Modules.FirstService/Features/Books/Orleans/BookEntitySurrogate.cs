namespace ModularMonolith.Modules.FirstService.Features.Books.Orleans;

[GenerateSerializer]
internal record BookEntitySurrogate(
  string Title,
  string Author,
  decimal Price);
