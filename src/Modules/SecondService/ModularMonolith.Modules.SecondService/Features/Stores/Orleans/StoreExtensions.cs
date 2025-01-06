using ModularMonolith.Modules.SecondService.Features.Stores.Data;

namespace ModularMonolith.Modules.SecondService.Features.Stores.Orleans;
internal static class StoreExtensions
{
  public static StoreEntity ToEntity(this StoreGrainState state, Guid id)
  {
    return new StoreEntity()
    {
      Id = id,
      Name = state.Name
    };
  }

  public static StoreGrainState ToState(this StoreEntity entity)
  {
    return new StoreGrainState(
       Name: entity.Name);
  }
}
