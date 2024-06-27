using System.Collections.Generic;

namespace Core.Model
{
    public abstract class EntitySystem<TEntity> : BaseEntitySystem, IModelSystem<TEntity>
        where TEntity : BaseEntity
    {
        public abstract void OnNew(TEntity newEntity);
        public abstract void OnDestroy(TEntity newEntity);
        public abstract void Update(TEntity entity, float deltaTime);

        internal override void Update(EntityLifetimeManager entityLifetimeManager, float deltaTime)
        {
            IEnumerable<TEntity> entities = entityLifetimeManager.GetAllEntitiesByType<TEntity>();
            foreach (TEntity entity in entities)
            {
                Update(entity, deltaTime);
            }
        }

        protected EntitySystem() : base(typeof(TEntity)) { }
    }
    
}