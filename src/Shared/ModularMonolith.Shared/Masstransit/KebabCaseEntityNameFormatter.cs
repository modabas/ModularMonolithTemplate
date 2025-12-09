using MassTransit;

namespace ModularMonolith.Shared.Masstransit;

public class KebabCaseEntityNameFormatter : KebabCaseEndpointNameFormatter, IEntityNameFormatter
{
  public KebabCaseEntityNameFormatter(string? prefix, bool includeNamespace)
    : base(prefix, includeNamespace)
  {
  }

  public string FormatEntityName<T>()
  {
    return GetMessageName(typeof(T));
  }
}
