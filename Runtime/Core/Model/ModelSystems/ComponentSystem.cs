using System;
using Core.Model.ModelSystems;
using Core.Systems;

namespace Core.Model
{
    
    public abstract class ComponentSystem<TComponent> : BaseComponentSystem
        where TComponent : class, IComponent 
    {
        public abstract void OnNewComponent(TComponent newComponent);
        public abstract void OnComponentDestroy(TComponent newComponent);
        public abstract void UpdateComponent(TComponent component, float deltaTime);

        internal override void Update(EntityLifetimeManager entityLifetimeManager, float deltaTime)
        {
            foreach (TComponent component in entityLifetimeManager.GetAllEntitiesWithComponent<TComponent>())
            {
                UpdateComponent(component, deltaTime);
            }
        }

        protected ComponentSystem() : base(typeof(TComponent)) { }
    }
    
    public abstract class BaseComponentSystem : IModelSystem
    {
        private const string DEFAULT_COMPONENT_SYSTEM_GROUP = "ComponentSystems";
        
        public bool Active { get; set; } = true;
        public SystemGroup Group { get; private set; } = new SystemGroup(DEFAULT_COMPONENT_SYSTEM_GROUP);
        
        protected BaseComponentSystem(Type componentType)
        {
            ComponentType = componentType;
        }

        internal abstract void Update(EntityLifetimeManager entityLifetimeManager, float deltaTime);

        public Type ComponentType { get; private set; }
        
    }
}