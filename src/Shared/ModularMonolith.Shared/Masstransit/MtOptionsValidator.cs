using FluentValidation;
using FluentValidation.Validators;

namespace ModularMonolith.Shared.Masstransit;

public class MtOptionsValidator : AbstractValidator<MtOptions>
{
  public MtOptionsValidator(IValidator<MtOptionsRabbitMq> rabbitMqValidator)
  {
    RuleFor(x => x.RabbitMq).NotNull().SetValidator(rabbitMqValidator);
  }
}

public class MtOptionsRabbitMqValidator : AbstractValidator<MtOptionsRabbitMq>
{
  public MtOptionsRabbitMqValidator()
  {
    RuleFor(x => x.Host).NotEmpty();
    RuleFor(x => x.Port).SetValidator(new GreaterThanValidator<MtOptionsRabbitMq, ushort>(0));
    RuleFor(x => x.VirtualHost).NotEmpty();
    RuleFor(x => x.Username).NotEmpty();
    RuleFor(x => x.Password).NotEmpty();
  }
}
