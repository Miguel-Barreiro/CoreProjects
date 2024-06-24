using System;
using Core.Model.ModelSystems;
using Core.Systems;

namespace Core.Model
{
    public abstract class EntitySystem<TEntity> : BaseEntitySystem
        where TEntity : BaseEntity
    {
        public abstract void OnNewEntity(TEntity newEntity);
        public abstract void OnDestroyEntity(TEntity newEntity);
        public abstract void UpdateEntity(TEntity entity, float deltaTime);

        protected EntitySystem() : base(typeof(TEntity)) { }
    }

    public abstract class BaseEntitySystem : IModelSystem
    {
        private const string DEFAULT_ENTITY_SYSTEM_GROUP = "EntitySystems";
        
        public bool Active { get; set;} = true;
        public SystemGroup Group { get; private set; } = new SystemGroup(DEFAULT_ENTITY_SYSTEM_GROUP);

        protected BaseEntitySystem(Type componentType)
        {
            EntityType = componentType;
        }
        
        public Type EntityType { get; private set; }

    }
}