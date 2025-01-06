namespace ModularMonolith.Modules.FirstService.Features.Books.Orleans;

[GenerateSerializer]
internal record BookGrainState(
  string Title,
  string Author,
  decimal Price);
