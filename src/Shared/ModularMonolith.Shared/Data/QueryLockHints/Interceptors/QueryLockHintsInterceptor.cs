using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ModularMonolith.Shared.Data.QueryLockHints.Interceptors;
public class QueryLockHintsInterceptor(bool checkFirstTagOnly) : DbCommandInterceptor()
{
  private const string LookFor = $"{QueryLockHintsDefinitions.TagPrefix} {QueryLockHintsDefinitions.HintPrefix}";
  public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
  {
    ManipulateCommand(command);
    return result;
  }

  public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
    DbCommand command,
    CommandEventData eventData,
    InterceptionResult<DbDataReader> result,
    CancellationToken cancellationToken = default)
  {
    ManipulateCommand(command);
    return new ValueTask<InterceptionResult<DbDataReader>>(result);
  }

  private void ManipulateCommand(DbCommand command)
  {
    var hint = GetQueryLockHint(command);
    if (!string.IsNullOrWhiteSpace(hint))
    {
      command.CommandText += $" {hint}";
    }
  }

  private string? GetQueryLockHint(DbCommand command)
  {
    if (checkFirstTagOnly)
    {
      //Check if command text starts with expected tag
      //If so, get query hint, else return null
      if (command.CommandText.StartsWith(LookFor, StringComparison.Ordinal))
      {
        var index = command.CommandText.IndexOfAny(['\r', '\n']);
        var line = index == -1 ? command.CommandText : command.CommandText.Substring(0, index);
        var hint = line.Substring(LookFor.Length).Trim();
        return hint;
      }
      return null;
    }
    else
    {
      //Check all lines of command text for expected tag
      //Get query hint from the first one, return null if none found
      using (var reader = new StringReader(command.CommandText))
      {
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
          if (line.StartsWith(LookFor, StringComparison.Ordinal))
          {
            var hint = line.Substring(LookFor.Length).Trim();
            if (!string.IsNullOrWhiteSpace(hint))
            {
              return hint;
            }
          }
        }
      }
      return null;
    }
  }
}
