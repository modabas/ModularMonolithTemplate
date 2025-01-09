using ModularMonolith.Modules.SecondService.Features.Stores.Data;

namespace ModularMonolith.Modules.SecondService.Features.Stores.Orleans;
internal static class StoreExtensions
{
  public static StoreEntity ToEntity(this StoreEntitySurrogate state, Guid id)
  {
    return new StoreEntity()
    {
      Id = id,
      Name = state.Name
    };
  }

  public static StoreEntitySurrogate ToSurrogate(this StoreEntity entity)
  {
    return new StoreEntitySurrogate(
       Name: entity.Name);
  }
}
