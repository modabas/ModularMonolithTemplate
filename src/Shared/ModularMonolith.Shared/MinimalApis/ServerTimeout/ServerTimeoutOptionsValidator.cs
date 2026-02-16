using FluentValidation;

namespace ModularMonolith.Shared.MinimalApis.ServerTimeout;

public class ServerTimeoutOptionsValidator : AbstractValidator<ServerTimeoutOptions>
{
  public ServerTimeoutOptionsValidator(IValidator<ServerTimeoutOptionsEndpoint> endpointValidator)
  {
    RuleFor(x => x.DefaultTimeout).GreaterThanOrEqualTo(TimeSpan.Zero);
    RuleForEach(x => x.Endpoints).NotNull()
      .SetValidator(endpointValidator);

  }
}

public class ServerTimeoutOptionsEndpointValidator : AbstractValidator<ServerTimeoutOptionsEndpoint>
{
  public ServerTimeoutOptionsEndpointValidator()
  {
    RuleFor(x => x.Type).NotEmpty();
    RuleFor(x => x.Timeout).GreaterThanOrEqualTo(TimeSpan.Zero);
  }
}
