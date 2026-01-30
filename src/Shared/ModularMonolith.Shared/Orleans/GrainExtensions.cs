using ModResults;

namespace ModularMonolith.Shared.Orleans;

public static class GrainExtensions
{
  extension(IGrainWithStringKey grain)
  {
    public Result<Guid> GetPrimaryKeyAsGuid()
    {
      var idString = grain.GetPrimaryKeyString();
      if (Guid.TryParse(idString, out var id))
      {
        return id;
      }
      return Result<Guid>.Error($"Invalid Guid: {idString}");
    }
  }
}
