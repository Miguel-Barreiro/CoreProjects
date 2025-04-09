
namespace Core.Model.ModelSystems
{
    
    public abstract class ComponentSystem<TComponent> : BaseEntitySystem, IModelSystem<TComponent>
        where TComponent : class, IComponent 
    {
        public abstract void OnNew(TComponent newComponent);
        public abstract void OnDestroy(TComponent destroyedComponent);
        public abstract void Update(TComponent component, float deltaTime);
        
        internal override void Update(EntitiesContainer entitiesContainer, float deltaTime)
        {
            foreach (TComponent component in entitiesContainer.GetAllEntitiesByType<TComponent>())
            {
                Update(component, deltaTime);
            }
        }
        
        protected ComponentSystem() : base(typeof(TComponent)) { }
    }
    
}