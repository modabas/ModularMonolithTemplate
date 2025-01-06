using ModularMonolith.Shared.Data.QueryLockHints.Enums;

namespace ModularMonolith.Shared.Data.QueryLockHints.Extensions;

public static class QueryLockBehaviorExtensions
{
  public static string GetSqlKeyword(this QueryLockBehavior lockBehavior)
  {
    return lockBehavior switch
    {
      QueryLockBehavior.None => string.Empty,
      QueryLockBehavior.NoWait => "NOWAIT",
      QueryLockBehavior.SkipLocked => "SKIP LOCKED",
      _ => string.Empty,
    };
  }
}
