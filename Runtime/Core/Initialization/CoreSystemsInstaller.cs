using Core.Events;
using Core.Model;
using Core.Systems;
using Core.Zenject.Source.Main;

namespace Core.Initialization
{
	public abstract class CoreSystemsInstaller : SystemsInstallerBase
    {

        public CoreSystemsInstaller(DiContainer container) : base(container) { }
        
        
        private EventQueue eventQueue = null;

        internal override void OnComplete()
        {
            base.OnComplete();
            eventQueue.Execute<OnProjectInstallCompleteEvent>();
        }

        protected override void InstallSystems()
        {
            TypeCache typeCache = BuildTypeCache();

            BuildEventManager();
            BuildGameLoopSystem();
            BuildEntityManager();
            BuildComponentSystemsLogic();
        }

        
        private void BuildGameLoopSystem()
        {
            SystemsController systemsController = new SystemsController();
            BindInstance(systemsController);
        }

        private void BuildEventManager()
        {
            if (eventQueue == null)
            {
                eventQueue = new EventQueue();
                BindInstance(eventQueue);
            }
        }
        


        private  TypeCache BuildTypeCache()
        {
            TypeCache typeCache = TypeCache.Get();
            BindInstance(typeCache);
            return typeCache;
        }

        private void BuildComponentSystemsLogic()
        {
            EntitySystemsContainer entitySystemsContainer = new EntitySystemsContainer();
            BindInstance(entitySystemsContainer);
        }


        
        protected void BuildEntityManager()
        {
            EntitiesContainer entityManagerComponent = EntitiesContainer.CreateInstance();
            BindInstance(entityManagerComponent);
        }        
		
	}
}