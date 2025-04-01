using Core.Core.Model.Data;
using Core.Systems;
using Core.View;
using Core.View.UI;
using Core.Zenject.Source.Main;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Initialization
{
	public class CoreSystemsInstallerForRuntime : CoreSystemsInstaller
	{
		private readonly Transform RootCoreParent;
		private readonly Canvas RootUIPrefab;

		internal CoreSystemsInstallerForRuntime(Canvas rootUIPrefab, Transform rootCoreParent, DiContainer container) : base(container)
		{
			RootCoreParent = rootCoreParent;
			RootUIPrefab = rootUIPrefab;
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

			BuildDataContainer();
		}

		private void BuildDataContainer()
		{
			DataConfigContainer configContainer = new DataConfigContainer();
			BindInstance(configContainer);
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
			
			GameObject newCanvasObj = GameObject.Instantiate(RootUIPrefab.gameObject, RootCoreParent);
			newCanvasObj.SetActive(true);
			Canvas rootCanvas = newCanvasObj.GetComponent<Canvas>();
			
			// GameObject newCanvasObj = new GameObject(UIRootImplementation.ROOT_CANVAS_NAME);
			// rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
			
			// CanvasScaler canvasScaler = newCanvasObj.AddComponent<CanvasScaler>();
			// canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			// canvasScaler.matchWidthOrHeight = 0;
			//
			// newCanvasObj.AddComponent<GraphicRaycaster>();
			
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

			KineticEntityUpdateViewSystem kineticEntityUpdateViewSystem = new();
			BindInstance(kineticEntityUpdateViewSystem);

			PhisycsEntitiesUpdateViewSystem phisycsEntitiesUpdateViewSystem = new();
			BindInstance(phisycsEntitiesUpdateViewSystem);
			
			TimeScaleSystemImplementation timeScaleSystemImplementation = new();
			BindInstance<TimeScaleSystem>(timeScaleSystemImplementation);
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