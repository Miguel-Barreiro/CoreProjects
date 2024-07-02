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
            BuildScenesController();
            BuildGameLoopSystem();
            BuildGenericGameobjePool();

            BuildEntityManager();
            BuildComponentSystemsLogic();
            BuildTypeCache();
            BuildSystemsManager();
        }
        
        private void BuildSystemsManager()
        {
            GameObject systemsManager = new GameObject("SystemsManager");
            SystemsManager systemsManagerComponent = systemsManager.AddComponent<SystemsManager>();
            DontDestroyOnLoad(systemsManager);

            BindInstance(systemsManagerComponent);
        }

        private void BuildTypeCache()
        {
            TypeCache typeCache = new TypeCache();
            BindInstance(typeCache);
        }

        private void BuildComponentSystemsLogic()
        {
            EntitySystemsContainer entitySystemsContainer = new EntitySystemsContainer();
            BindInstance(entitySystemsContainer);
        }

        protected override void OnComplete()
        {
            installComplete = true;
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
            EntityLifetimeManager entityManagerComponent = EntityLifetimeManager.CreateInstance();
            BindInstance(entityManagerComponent);
        }        
    }
}