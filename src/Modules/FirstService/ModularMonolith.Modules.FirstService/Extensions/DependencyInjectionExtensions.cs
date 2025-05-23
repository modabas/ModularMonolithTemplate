using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModEndpoints;
using ModularMonolith.Modules.FirstService.Data;
using ModularMonolith.Modules.FirstService.Features.Books;
using ModularMonolith.Shared.Data;
using ModularMonolith.Shared.Data.QueryLockHints.Extensions;
using ModularMonolith.Shared.Data.SimpleOutbox.Extensions;
using Npgsql;

namespace ModularMonolith.Modules.FirstService.Extensions;
internal static class DependencyInjectionExtensions
{
  public static WebApplicationBuilder AddFirstServiceModule(this WebApplicationBuilder builder)
  {
    builder.AddDatabase();
    builder.Services.AddHostedService<PgDbMigrationService<FirstServiceDbContext>>();
    builder.AddSimpleOutbox<FirstServiceDbContext>();
    builder.Services.AddModEndpointsFromAssemblyContaining<GetBookById>();
    builder.Services.AddValidatorsFromAssemblyContaining<GetBookByIdRequestValidator>(includeInternalTypes: true);

    return builder;
  }

  public static IBusRegistrationConfigurator AddFirstServiceModule(this IBusRegistrationConfigurator bus)
  {
    bus.AddConsumers(typeof(DependencyInjectionExtensions).Assembly.GetTypes());
    return bus;
  }

  private static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
  {
    var dataSourceName = "FirstServiceDataSource";
    builder.Services.AddDbDataSource(dataSourceName,
      (_, _) =>
      {
        var connectionString = builder.Configuration.GetConnectionString("FirstServiceConnectionString");
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        return dataSourceBuilder.Build();
      });

    builder.Services.RegisterDbContext<FirstServiceDbContext>(
      (serviceProvider, optionsBuilder) =>
      {
        optionsBuilder
          .UseNpgsql(
            serviceProvider.GetRequiredKeyedService<NpgsqlDataSource>(dataSourceName),
            b =>
            {
              var assemblyName = typeof(FirstServiceDbContext).Assembly.GetName().Name;
              b.MigrationsAssembly(assemblyName);
              b.MigrationsHistoryTable("__EFMigrationsHistory", FirstServiceDbContext.Schema);
            })
          .WithQueryLockHints();
        //optionsBuilder.LogTo(Console.WriteLine);
      });
    return builder;
  }
}
