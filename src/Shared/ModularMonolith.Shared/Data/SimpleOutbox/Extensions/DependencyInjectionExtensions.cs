using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModularMonolith.Shared.Data.SimpleOutbox.Abstractions;
using ModularMonolith.Shared.Data.SimpleOutbox.Jobs;
using ModularMonolith.Shared.Masstransit.SimpleOutbox;
using ModularMonolith.Shared.Options.Validation;

namespace ModularMonolith.Shared.Data.SimpleOutbox.Extensions;

public static class DependencyInjectionExtensions
{
  public static IHostApplicationBuilder AddSimpleOutbox<TDbContext>(
    this IHostApplicationBuilder builder,
    string configurationKey = SimpleOutboxDefinitions.DefaultConfigurationKey)
    where TDbContext : DbContext, IOutboxDbContext
  {
    builder.Services.AddOptionsWithFluentValidation<SimpleOutboxOptions>()
      .Bind(builder.Configuration.GetSection(configurationKey));
    builder.Services.AddScoped<IValidator<SimpleOutboxOptions>, SimpleOutboxOptionsValidator>();
    builder.Services.AddHostedService<OutboxMessageRemovalService<TDbContext>>();
    builder.Services.AddHostedService<OutboxMessagePublisherService<TDbContext>>();
    builder.AddMasstransitOutboxPublisher();
    return builder;
  }
}
