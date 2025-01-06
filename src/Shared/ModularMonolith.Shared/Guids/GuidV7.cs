using UUIDNext;

namespace ModularMonolith.Shared.Guids;
public static class GuidV7
{
  public static Guid CreateVersion7()
  {
    return Uuid.NewDatabaseFriendly(Database.PostgreSql);
  }
}
