using System.Collections.Generic;

namespace Core.Model
{
    // public abstract class EntitySystem<TEntity> : BaseEntitySystem 
    //     where TEntity : Entity
    // {
    //     public abstract void OnNew(TEntity newEntity);
    //     public abstract void OnDestroy(TEntity newEntity);
    //     
    //     // public abstract void Update(TEntity entity, float deltaTime);
    //     //
    //     // internal override void Update(EntitiesContainer entitiesContainerOld, float deltaTime)
    //     // {
    //     //     IEnumerable<TEntity> entities = entitiesContainerOld.GetAllEntitiesByType<TEntity>();
    //     //     foreach (TEntity entity in entities)
    //     //     {
    //     //         Update(entity, deltaTime);
    //     //     }
    //     // }
    //
    //     protected EntitySystem() : base(typeof(TEntity)) { }
    // }
    //
    //
    // public interface IEntitySystem { }
    //
    // public interface OnEntityCreated<TEntity> : IEntitySystem
    //     where TEntity : Entity
    // {
    //     public void OnEntityCreated(TEntity newEntity);
    // }
    //
    // public interface OnEntityDestroyed<TEntity>: IEntitySystem
    //     where TEntity : Entity
    // {
    //     public void OnEntityDestroyed(TEntity newEntity);
    // }
    //
    // public interface UpdateEntitiesSystem<TEntity>: IEntitySystem
    //     where TEntity : Entity
    // {
    //     public void UpdateEntities(IEnumerable<TEntity> newEntity);
    // }
    //
}