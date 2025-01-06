using ModularMonolith.Shared.Guids;

namespace ModularMonolith.Shared.Data;

public abstract class BaseEntity
{
  public Guid Id { get; set; } = GuidV7.CreateVersion7();
}
