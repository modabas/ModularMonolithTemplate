using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Data.QueryLockHints.Enums;

namespace ModularMonolith.Shared.Data.QueryLockHints.Extensions;

public static class QueryableExtensions
{
  public static IQueryable<T> WithQueryLock<T>(this IQueryable<T> query, QueryLockStrength lockStrength, QueryLockBehavior lockBehavior = QueryLockBehavior.None)
  {
    return query.TagWith($"{QueryLockHintsDefinitions.HintPrefix} FOR {lockStrength.GetSqlKeyword()} {lockBehavior.GetSqlKeyword()}".TrimEnd());
  }
}
