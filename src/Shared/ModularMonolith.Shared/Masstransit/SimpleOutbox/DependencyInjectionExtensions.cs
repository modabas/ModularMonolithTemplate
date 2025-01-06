using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ModularMonolith.Shared.Data.SimpleOutbox.Abstractions;

namespace ModularMonolith.Shared.Masstransit.SimpleOutbox;

public static class DependencyInjectionExtensions
{
  public static IHostApplicationBuilder AddMasstransitOutboxPublisher(this IHostApplicationBuilder builder)
  {
    builder.Services.TryAddKeyedTransient<IOutboxPublisher, MtOutboxPublisher>(MtSimpleOutboxDefinitions.PublisherName);
    return builder;
  }
}
