using Core.Systems;
using Core.View;
using Core.View.UI;
using Core.Zenject.Source.Main;
using UnityEngine;

namespace Core.Initialization
{
	public class CoreSystemsInstallerForRuntime : CoreSystemsInstaller
	{
		private readonly Transform RootCoreParent;

		internal CoreSystemsInstallerForRuntime(Transform rootCoreParent, DiContainer container) : base(container)
		{
			RootCoreParent = rootCoreParent;
		}
		
		protected override void InstallSystems()
		{
			base.InstallSystems();
			
			BuildViewSystems();
			BuildGenericGameobjePool();
			BuildScenesController();

			BuildSystemsManager();
			
			BuildUISystems();

			SetSystemsControllerForRuntime();
		}

		
		private void SetSystemsControllerForRuntime()
		{
			SystemsController systemsController = Container.Resolve<SystemsController>();
			systemsController.SetMode(SystemsController.SystemsControllerMode.AUTOMATIC);
		}
		
		// private void BuildGameLoopRunner()
		// {
		// 	
		// 	GameObject systemsManager = new GameObject("SystemsLoop");
		// 	SystemsManager systemsManagerComponent = systemsManager.AddComponent<SystemsManager>();
		// 	GameObject.DontDestroyOnLoad(systemsManager);
		//
		// 	BindInstance(systemsManagerComponent);
		// }

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
		
		
		private void BuildSystemsManager()
		{
			GameObject systemsManager = new GameObject("[Systems Manager]");
			SystemsManager systemsManagerComponent = systemsManager.AddComponent<SystemsManager>();
			GameObject.DontDestroyOnLoad(systemsManager);

			BindInstance(systemsManagerComponent);
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

		protected void BuildGenericGameobjePool()
		{
			GameObject gameLoop = new GameObject("GameObjectPool");
			GenericGameObjectPool genericGameObjectPool = gameLoop.AddComponent<GenericGameObjectPool>();
			gameLoop.transform.SetParent(RootCoreParent);
			BindInstance(genericGameObjectPool);
		}

		protected void BuildScenesController()
		{
			if (!Container.HasBinding<ScenesController>())
			{
				ScenesControllerImplemention scenesController = new ScenesControllerImplemention();
				BindInstance<ScenesController>(scenesController);
			}
		}


	}
}