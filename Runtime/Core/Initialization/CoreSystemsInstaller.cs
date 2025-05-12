using System;
using Core.Events;
using Core.Model;
using Core.Model.ModelSystems;
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
            EntitiesContainer entitiesContainer = EntitiesContainer.CreateInstance();
            BindInstance(entitiesContainer);

            BuildDataComponentContainers();
            
            BuildEventManager();
            BuildEntityEventManagers();
            
            BuildGameLoopSystem();
            BuildEntityManager();

            BuildTimerSystem();
            BuildStatsSystem();

        }

        private void BuildEntityEventManagers()
        {
            EntityEventQueuesContainer entityEventQueuesContainer = EntityEventQueuesContainer.Get();
            BindInstance(entityEventQueuesContainer);
            
            foreach ((Type entityEventType,BaseEntityEventQueueImplementation queue) in entityEventQueuesContainer.GetAllEntityEventQueues())
            {
                Type queueType = typeof(IEntityEventQueue<>).MakeGenericType(entityEventType);
                BindInstanceByDynamicType(queue, queueType);
            }
            
        }

        private void BuildDataComponentContainers()
        {
            DataContainersControllerImplementation dataContainersController = DataContainersControllerImplementation.GetInstance(); 
            BindInstance<DataContainersController>(dataContainersController);
            BindInstance<DataContainersControllerImplementation>(dataContainersController);
            
            foreach ((object container, Type _, Type containerType) in dataContainersController.GetAllComponentContainers())
            {
                BindInstanceByDynamicType(container, containerType);
            }
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

        
        protected void BuildEntityManager()
        {
            // EntitiesContainerImplementation entityManager = new EntitiesContainerImplementation();
            // BindInstance<EntitiesContainer>(entityManager);
            
            // EntitiesContainer_old entityManagerComponent = EntitiesContainer_old.CreateInstance();
            // BindInstance(entityManagerComponent);
        }        
		
	}
}