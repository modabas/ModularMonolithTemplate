using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModularMonolith.Shared.Data.SimpleOutbox.Abstractions;
using ModularMonolith.Shared.Data.SimpleOutbox.Jobs;
using ModularMonolith.Shared.Masstransit.SimpleOutbox;

namespace ModularMonolith.Shared.Data.SimpleOutbox.Extensions;

public static class DependencyInjectionExtensions
{
  public static IHostApplicationBuilder AddSimpleOutbox<TDbContext>(
    this IHostApplicationBuilder builder,
    string configurationKey = SimpleOutboxDefinitions.DefaultConfigurationKey)
    where TDbContext : DbContext, IOutboxDbContext
  {
    builder.Services.Configure<SimpleOutboxSettings>(options =>
    {
      builder.Configuration.Bind(configurationKey, options);
    });
    builder.Services.AddHostedService<OutboxMessageRemovalService<TDbContext>>();
    builder.Services.AddHostedService<OutboxMessagePublisherService<TDbContext>>();
    builder.AddMasstransitOutboxPublisher();
    return builder;
  }
}
