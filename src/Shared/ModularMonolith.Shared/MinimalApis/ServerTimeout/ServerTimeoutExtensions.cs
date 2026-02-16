using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ModEndpoints.Core;
using ModularMonolith.Shared.Data.SimpleOutbox;
using ModularMonolith.Shared.Options.Validation;

namespace ModularMonolith.Shared.MinimalApis.ServerTimeout;

public static class ServerTimeoutExtensions
{
  public static IHostApplicationBuilder AddServerTimeout(this IHostApplicationBuilder builder)
  {
    builder.Services.AddOptionsWithFluentValidation<ServerTimeoutOptions>()
      .Bind(builder.Configuration.GetSection("ServerTimeout"));
    builder.Services.AddScoped<IValidator<ServerTimeoutOptions>, ServerTimeoutOptionsValidator>();
    builder.Services.AddScoped<IValidator<ServerTimeoutOptionsEndpoint>, ServerTimeoutOptionsEndpointValidator>();
    return builder;
  }

  public static ServerTimeoutEndpointSetting GetDefinition(this ServerTimeoutOptions options, IEndpointConfiguratorMarker endpoint)
  {
    var endpointType = endpoint.GetType().FullName;
    if (string.IsNullOrWhiteSpace(endpointType))
    {
      return new ServerTimeoutEndpointSetting(false, options.DefaultTimeout);
    }
    var endpointSetting = options.GetEndpointSetting(endpointType);
    if (endpointSetting is null)
    {
      return new ServerTimeoutEndpointSetting(false, options.DefaultTimeout);
    }

    return new ServerTimeoutEndpointSetting(true, endpointSetting.Timeout);
  }

  public static TBuilder WithServerTimeout<TBuilder>(this TBuilder builder, IServiceProvider serviceProvider, IEndpointConfiguratorMarker endpoint) where TBuilder : IEndpointConventionBuilder
  {
    var definition = serviceProvider
      .GetService<IOptions<ServerTimeoutOptions>>()?
      .Value
      .GetDefinition(endpoint);
    if (definition is not null)
    {
      builder.WithMetadata(new ServerTimeoutMetadata(definition));
    }
    return builder;
  }
}
