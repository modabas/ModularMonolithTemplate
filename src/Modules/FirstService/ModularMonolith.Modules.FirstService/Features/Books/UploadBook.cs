using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModEndpoints;
using ModEndpoints.Core;
using ModResults;
using ModularMonolith.Modules.FirstService.FeatureContracts.Features;
using ModularMonolith.Modules.FirstService.Features.Books.Configuration;

namespace ModularMonolith.Modules.FirstService.Features.Books;

internal record UploadBookRequest(string Title, [FromForm] string Author, IFormFile BookFile);

internal class UploadBookRequestValidator : AbstractValidator<UploadBookRequest>
{
  public UploadBookRequestValidator()
  {
    RuleFor(x => x.Title).NotEmpty();
    RuleFor(x => x.Author).NotEmpty();
    RuleFor(x => x.BookFile).NotEmpty();
  }
}

[MapToGroup<BooksV2RouteGroup>()]
internal class UploadBook
  : WebResultEndpoint<UploadBookRequest, UploadBookResponse>
{
  private const string Pattern = "/upload/{Title}";

  protected override void Configure(
    EndpointConfigurationBuilder builder,
    EndpointConfigurationContext configurationContext)
  {
    builder.MapPost(Pattern)
      .DisableAntiforgery()
      .Produces<UploadBookResponse>();
  }

  protected override Task<WebResult<UploadBookResponse>> HandleAsync(
    UploadBookRequest req,
    CancellationToken ct)
  {
    return Task.FromResult(
      WebResults.FromResult(
        new UploadBookResponse(
          req.BookFile.FileName,
          req.BookFile.Length)));
  }
}
