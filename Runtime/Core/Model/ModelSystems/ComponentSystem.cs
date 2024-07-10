
namespace Core.Model.ModelSystems
{
    
    public abstract class ComponentSystem<TComponent> : BaseEntitySystem, IModelSystem<TComponent>
        where TComponent : class, IComponent 
    {
        public abstract void OnNew(TComponent newComponent);
        public abstract void OnDestroy(TComponent newComponent);
        public abstract void Update(TComponent component, float deltaTime);
        
        internal override void Update(EntityLifetimeManager entityLifetimeManager, float deltaTime)
        {
            foreach (TComponent component in entityLifetimeManager.GetAllEntitiesByType<TComponent>())
            {
                Update(component, deltaTime);
            }
        }
        
        protected ComponentSystem() : base(typeof(TComponent)) { }
    }
    
}