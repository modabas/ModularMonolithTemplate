using System.Data.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Shared.Data;

public static class DependencyInjectionExtensions
{
  public static IServiceCollection AddDbDataSource<TDataSource>(
    this IServiceCollection services,
    object serviceKey,
    Func<IServiceProvider, object?, TDataSource> configureDataSource)
    where TDataSource : DbDataSource
  {
    services.AddKeyedSingleton(
        serviceKey,
        (provider, options) =>
        {
          return configureDataSource.Invoke(provider, options);
        });
    return services;
  }

  public static IServiceCollection RegisterDbContext<TDbContext>(
    this IServiceCollection services,
    Action<IServiceProvider, DbContextOptionsBuilder> configureDbContextOptionsBuilder)
    where TDbContext : DbContext
  {
    //Register Db context
    services.AddDbContextPool<TDbContext>((provider, options) =>
    {
      configureDbContextOptionsBuilder?.Invoke(provider, options);
      options.UseSnakeCaseNamingConvention();
    });
    return services;
  }
}
