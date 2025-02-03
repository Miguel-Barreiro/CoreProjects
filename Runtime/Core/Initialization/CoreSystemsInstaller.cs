using Core.Events;
using Core.Model;
using Core.Systems;
using Core.View;
using Core.View.UI;
using Core.Zenject.Source.Main;
using UnityEngine;

namespace Core.Initialization
{
	internal class CoreSystemsInstaller : SystemsInstallerBase
    {
        
        protected readonly Transform RootCoreParent;

        public CoreSystemsInstaller(Transform rootCoreParent, DiContainer container) : base(container)
        {
            RootCoreParent = rootCoreParent;
        }
        

        protected override void InstallSystems()
        {
			TypeCache typeCache = BuildTypeCache();

            BuildEventManager();
			BuildGameLoopSystem();
			BuildEntityManager();
			BuildComponentSystemsLogic();
			BuildSystemsManager();

			BuildViewSystems();
			BuildGenericGameobjePool();
			BuildScenesController();
            
			BuildUISystems();
        }

        protected void BuildUISystems()
        {
            GameObject newCanvasObj = new GameObject(UIRootImplementation.ROOT_CANVAS_NAME);
            Canvas rootCanvas = newCanvasObj.AddComponent<Canvas>();
            newCanvasObj.SetActive(true);
            
            UIRootImplementation uiRootImplementation = new UIRootImplementation(rootCanvas);
            BindInstance(uiRootImplementation);
            BindInstance<UIRoot>(uiRootImplementation);

            GameObject.DontDestroyOnLoad(newCanvasObj);
        }

        private EventQueue eventQueue = null;

        internal override void OnComplete()
        {
            base.OnComplete();
            eventQueue.Execute<OnProjectInstallCompleteEvent>();
        }

        protected void BuildEventManager()
        {
            if (eventQueue == null)
            {
                eventQueue = new EventQueue();
                BindInstance(eventQueue);
            }
        }

        protected void BuildViewSystems()
        {
            ViewEntitiesContainer viewEntitiesContainer = new ViewEntitiesContainer();
            BindInstance(viewEntitiesContainer);

            PositionEntityUpdateViewSystem positionEntityUpdateViewSystem = new();
            BindInstance(positionEntityUpdateViewSystem);

            PhisycsEntitiesUpdateViewSystem phisycsEntitiesUpdateViewSystem = new();
            BindInstance(phisycsEntitiesUpdateViewSystem);
            
        }

        protected void BuildSystemsManager()
        {
            GameObject systemsManager = new GameObject("SystemsManager");
            SystemsManager systemsManagerComponent = systemsManager.AddComponent<SystemsManager>();
            GameObject.DontDestroyOnLoad(systemsManager);

            BindInstance(systemsManagerComponent);
        }

        protected TypeCache BuildTypeCache()
        {
            TypeCache typeCache = TypeCache.Get();
            BindInstance(typeCache);
            return typeCache;
        }

        protected void BuildComponentSystemsLogic()
        {
            EntitySystemsContainer entitySystemsContainer = new EntitySystemsContainer();
            BindInstance(entitySystemsContainer);
        }


        protected void BuildScenesController()
        {
            if (!Container.HasBinding<ScenesController>())
            {
                ScenesController scenesController = new ScenesController();
                BindInstance(scenesController);
            }
        }

        protected void BuildGameLoopSystem()
        {
            GameObject gameLoop = new GameObject("GameLoopSystem");
            SystemsController systemsController = gameLoop.AddComponent<SystemsController>();
            gameLoop.transform.SetParent(RootCoreParent);
            BindInstance(systemsController);
        }

        protected void BuildGenericGameobjePool()
        {
            GameObject gameLoop = new GameObject("GameObjectPool");
            GenericGameObjectPool genericGameObjectPool = gameLoop.AddComponent<GenericGameObjectPool>();
            gameLoop.transform.SetParent(RootCoreParent);
            BindInstance(genericGameObjectPool);
        }

        protected void BuildEntityManager()
        {
            EntitiesContainer entityManagerComponent = EntitiesContainer.CreateInstance();
            BindInstance(entityManagerComponent);
        }        
		
	}
}