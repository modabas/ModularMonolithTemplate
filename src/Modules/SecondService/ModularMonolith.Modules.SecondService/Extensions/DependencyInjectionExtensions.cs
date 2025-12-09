using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModEndpoints;
using ModularMonolith.Modules.SecondService.Data;
using ModularMonolith.Modules.SecondService.Features.Stores;
using ModularMonolith.Shared.Data;
using ModularMonolith.Shared.Data.QueryLockHints.Extensions;
using ModularMonolith.Shared.Data.SimpleOutbox.Extensions;
using Npgsql;

namespace ModularMonolith.Modules.SecondService.Extensions;

internal static class DependencyInjectionExtensions
{
  public static WebApplicationBuilder AddSecondServiceModule(this WebApplicationBuilder builder)
  {
    builder.AddDatabase();
    builder.Services.AddHostedService<PgDbMigrationService<SecondServiceDbContext>>();
    builder.AddSimpleOutbox<SecondServiceDbContext>();
    builder.Services.AddModEndpointsFromAssemblyContaining<GetStoreById>();
    builder.Services.AddValidatorsFromAssemblyContaining<GetStoreByIdRequestValidator>(includeInternalTypes: true);

    return builder;
  }

  public static IBusRegistrationConfigurator AddSecondServiceModule(this IBusRegistrationConfigurator bus)
  {
    bus.AddConsumers(typeof(DependencyInjectionExtensions).Assembly.GetTypes());
    return bus;
  }

  private static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
  {
    var dataSourceName = "SecondServiceDataSource";
    builder.Services.AddDbDataSource(dataSourceName,
      (_, _) =>
      {
        var connectionString = builder.Configuration.GetConnectionString("SecondServiceConnectionString");
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        return dataSourceBuilder.Build();
      });

    builder.Services.RegisterDbContext<SecondServiceDbContext>(
      (serviceProvider, optionsBuilder) =>
      {
        optionsBuilder
          .UseNpgsql(
            serviceProvider.GetRequiredKeyedService<NpgsqlDataSource>(dataSourceName),
            b =>
            {
              var assemblyName = typeof(SecondServiceDbContext).Assembly.GetName().Name;
              b.MigrationsAssembly(assemblyName);
              b.MigrationsHistoryTable("__EFMigrationsHistory", SecondServiceDbContext.Schema);
            })
          .WithQueryLockHints();
        //optionsBuilder.LogTo(Console.WriteLine);
      });
    return builder;
  }
}
