using System;
using Core.Systems;

namespace Core.Model
{
    public abstract class BaseEntitySystem : ISystem
    {
        
        public bool Active { get; set; } = true;
    
        public abstract SystemGroup Group { get; }
        
        protected BaseEntitySystem(Type componentType)
        {
            EntityType = componentType;
        }
    
        // internal abstract void Update(EntitiesContainer_old entitiesContainerOld, float deltaTime);
    
        public Type EntityType { get; private set; }
        
    }
}