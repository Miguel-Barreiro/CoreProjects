using Core.Events;
using Core.Model;
using Core.Model.Time;
using Core.Systems;
using Core.Zenject.Source.Main;

namespace Core.Initialization
{
	public abstract class CoreSystemsInstaller : SystemsInstallerBase
    {
        protected CoreSystemsInstaller(DiContainer container) : base(container) { }
        
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
            BuildTimerSystem();
            BuildStatsSystem();
        }

        private void BuildStatsSystem()
        {
            StatsSystemImplementation statsSystem = new StatsSystemImplementation();
            BindInstance<StatsSystem>(statsSystem);
            BindInstance<StatsSystemRo>(statsSystem);
        }

        private void BuildTimerSystem()
        {
            TimerSystemImplementation timerSystemImplementation = new TimerSystemImplementation();
            BindInstance<TimerSystemRo>(timerSystemImplementation);
            BindInstance<TimerSystem>(timerSystemImplementation);
            BindInstance<ITimerSystemImplementationInternal>(timerSystemImplementation);
            BindInstance(new TimerModel());
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