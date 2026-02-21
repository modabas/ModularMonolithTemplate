using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ModularMonolith.Shared.Options.Validation;

public sealed class FluentValidationOptionsValidator<TOptions>
    : IValidateOptions<TOptions> where TOptions : class
{
  private readonly IServiceProvider _serviceProvider;
  private readonly string? _defaultValidatorKey;
  private readonly string _optionsTypeName = typeof(TOptions).Name;
  public FluentValidationOptionsValidator(
    IServiceProvider serviceProvider,
    string? defaultValidatorKey)
  {
    _serviceProvider = serviceProvider;
    _defaultValidatorKey = defaultValidatorKey;
  }

  public ValidateOptionsResult Validate(string? name, TOptions options)
  {
    using (var scope = _serviceProvider.CreateScope())
    {
      var validator = GetValidator(scope, name);
      var validationResult = validator.Validate(options);

      ValidateOptionsResultBuilder resultBuilder = new ValidateOptionsResultBuilder();
      if (!validationResult.IsValid)
      {
        var propertyNamePrefix =
          string.IsNullOrWhiteSpace(name) ? _optionsTypeName : $"{_optionsTypeName} (named '{name}')";
        foreach (var error in validationResult.Errors)
        {
          resultBuilder.AddError(error.ErrorMessage, $"{propertyNamePrefix}.{error.PropertyName}");
        }
      }
      return resultBuilder.Build();
    }

    IValidator<TOptions> GetValidator(IServiceScope scope, string? name)
    {
      // Try to get a validator for the specific name.
      var validator = string.IsNullOrWhiteSpace(name)
        ? scope.ServiceProvider.GetService<IValidator<TOptions>>()
        : scope.ServiceProvider.GetKeyedService<IValidator<TOptions>>(name);
      // If a validator is not found for the specific name, fall back to the default validator.
      validator ??= string.IsNullOrWhiteSpace(_defaultValidatorKey)
        ? scope.ServiceProvider.GetRequiredService<IValidator<TOptions>>()
        : scope.ServiceProvider.GetRequiredKeyedService<IValidator<TOptions>>(_defaultValidatorKey);
      return validator;
    }
  }
}
