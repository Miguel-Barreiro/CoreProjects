using Core.Systems;
using UnityEngine;

namespace Core.Initialization
{
    public abstract class ProjectInstaller : BaseInstaller
    {
        protected override void InstantiateInternalSystems() 
        {
            BuildScenesController();
            BuildGameLoopSystem();
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

    }
}