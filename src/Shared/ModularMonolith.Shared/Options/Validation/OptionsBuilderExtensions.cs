using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace ModularMonolith.Shared.Options.Validation;

public static class OptionsBuilderExtensions
{
  public static OptionsBuilder<TOptions> ValidateWithFluentValidation<TOptions>(
    this OptionsBuilder<TOptions> optionsBuilder, string? defaultValidatorKey = null) where TOptions : class
  {
    optionsBuilder.Services.TryAddSingleton<IValidateOptions<TOptions>>(
        provider => new FluentValidationOptionsValidator<TOptions>(provider, defaultValidatorKey));
    return optionsBuilder;
  }

  public static OptionsBuilder<TOptions>? TryAddOptionsWithFluentValidationOnStart<TOptions>(
    this IServiceCollection services) where TOptions : class
  {
    if (!services.Any(d => d.ServiceType == typeof(IConfigureOptions<TOptions>)))
    {
      return services.AddOptions<TOptions>()
        .ValidateWithFluentValidation()
        .ValidateOnStart();
    }
    return null;
  }
}
