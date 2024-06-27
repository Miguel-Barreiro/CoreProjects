using System;
using Core.Systems;

namespace Core.Model
{
    public abstract class BaseEntitySystem : ISystem
    {
        private const string DEFAULT_ENTITY_SYSTEM_GROUP = "ModelSystems";
        
        public bool Active { get; set; } = true;
        public SystemGroup Group { get; private set; } = new SystemGroup(DEFAULT_ENTITY_SYSTEM_GROUP);
        
        protected BaseEntitySystem(Type componentType)
        {
            EntityType = componentType;
        }

        internal abstract void Update(EntityLifetimeManager entityLifetimeManager, float deltaTime);

        public Type EntityType { get; private set; }
        
    }
}