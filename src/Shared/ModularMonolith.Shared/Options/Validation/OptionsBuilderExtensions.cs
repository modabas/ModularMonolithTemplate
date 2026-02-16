using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ModularMonolith.Shared.Options.Validation;

public static class OptionsBuilderExtensions
{
  public static OptionsBuilder<TOptions> ValidateWithFluentValidation<TOptions>(
    this OptionsBuilder<TOptions> optionsBuilder) where TOptions : class
  {
    optionsBuilder.Services.AddSingleton<IValidateOptions<TOptions>>(
        provider => new FluentValidationOptionsValidator<TOptions>(
          optionsBuilder.Name, provider));
    return optionsBuilder;
  }

  public static OptionsBuilder<TOptions> AddOptionsWithFluentValidation<TOptions>(
    this IServiceCollection services) where TOptions : class
  {
    return services.AddOptions<TOptions>()
      .ValidateWithFluentValidation()
      .ValidateOnStart();
  }
}
