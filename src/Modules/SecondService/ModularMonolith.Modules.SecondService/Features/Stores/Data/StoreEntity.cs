using ModularMonolith.Shared.Data;

namespace ModularMonolith.Modules.SecondService.Features.Stores.Data;

internal class StoreEntity : BaseEntity
{
  public string Name { get; set; } = string.Empty;
}
