﻿using Core.Events;
using Core.Model;
using Core.Systems;
using Core.View;
using UnityEngine;

namespace Core.Initialization
{
    public abstract class ProjectInstaller : BaseInstaller
    {
        public override bool InstallComplete => installComplete;
        private bool installComplete = false;
        
        protected override void InstantiateInternalSystems() 
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
        }

        private EventQueue eventQueue = null;
        protected override void OnComplete()
        {
            installComplete = true;
            eventQueue.Execute<OnProjectInstallCompleteEvent>();
        }

        private void BuildEventManager()
        {
            if (eventQueue == null)
            {
                eventQueue = new EventQueue();
                BindInstance(eventQueue);
            }
        }

        private void BuildViewSystems()
        {
            ViewEntitiesContainer viewEntitiesContainer = new ViewEntitiesContainer();
            BindInstance(viewEntitiesContainer);

            PositionEntityUpdateViewSystem positionEntityUpdateViewSystem = new();
            BindInstance(positionEntityUpdateViewSystem);

            PhisycsEntitiesUpdateViewSystem phisycsEntitiesUpdateViewSystem = new();
            BindInstance(phisycsEntitiesUpdateViewSystem);
        }

        private void BuildSystemsManager()
        {
            GameObject systemsManager = new GameObject("SystemsManager");
            SystemsManager systemsManagerComponent = systemsManager.AddComponent<SystemsManager>();
            DontDestroyOnLoad(systemsManager);

            BindInstance(systemsManagerComponent);
        }

        private TypeCache BuildTypeCache()
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
        

        private void BuildScenesController()
        {
            if (!Container.HasBinding<ScenesController>())
            {
                ScenesController scenesController = new ScenesController();
                BindInstance(scenesController);
            }
        }
        
        private void BuildGameLoopSystem()
        {
            GameObject gameLoop = new GameObject("GameLoopSystem");
            SystemsController systemsController = gameLoop.AddComponent<SystemsController>();
            gameLoop.transform.SetParent(transform);
            BindInstance(systemsController);
        }
        
        private void BuildGenericGameobjePool()
        {
            GameObject gameLoop = new GameObject("GameObjectPool");
            GenericGameObjectPool genericGameObjectPool = gameLoop.AddComponent<GenericGameObjectPool>();
            gameLoop.transform.SetParent(transform);
            BindInstance(genericGameObjectPool);
        }

        private void BuildEntityManager()
        {
            EntitiesContainer entityManagerComponent = EntitiesContainer.CreateInstance();
            BindInstance(entityManagerComponent);
        }        
    }
}