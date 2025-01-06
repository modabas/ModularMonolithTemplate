using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Data.QueryLockHints.Interceptors;

namespace ModularMonolith.Shared.Data.QueryLockHints.Extensions;
public static class OptionsBuilderExtensions
{
  public static DbContextOptionsBuilder WithQueryLockHints(
    this DbContextOptionsBuilder builder,
    bool checkFirstTagOnly = true)
  {
    builder.AddInterceptors(new QueryLockHintsInterceptor(checkFirstTagOnly));
    return builder;
  }
}
