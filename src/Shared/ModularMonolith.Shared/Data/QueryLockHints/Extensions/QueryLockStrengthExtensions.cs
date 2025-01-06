using ModularMonolith.Shared.Data.QueryLockHints.Enums;

namespace ModularMonolith.Shared.Data.QueryLockHints.Extensions;

public static class QueryLockStrengthExtensions
{
  public static string GetSqlKeyword(this QueryLockStrength lockBehavior)
  {
    return lockBehavior switch
    {
      QueryLockStrength.Update => "UPDATE",
      QueryLockStrength.NoKeyUpdate => "NO KEY UPDATE",
      QueryLockStrength.Share => "SHARE",
      QueryLockStrength.KeyShare => "KEY SHARE",
      _ => throw new System.ComponentModel.InvalidEnumArgumentException(nameof(lockBehavior), (int)lockBehavior, typeof(QueryLockStrength))
    };
  }
}
