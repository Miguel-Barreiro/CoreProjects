using Core.Model;
using Core.Zenject.Source.Main;
using UnityEngine;

namespace Core.Initialization
{
	internal class CoreSystemsForTestsInstaller : CoreSystemsInstaller
	{
		public CoreSystemsForTestsInstaller(Transform rootCoreParent, DiContainer container) : base(rootCoreParent, container) { }
		
		protected override void InstallSystems()
		{
			TypeCache typeCache = BuildTypeCache();

			BuildEventManager();
			BuildGameLoopSystem();

			BuildEntityManager();
			BuildComponentSystemsLogic();
			BuildSystemsManager();

			//for tests we dont use these systems
			
//			BuildViewSystems();
//			BuildGenericGameobjePool();
//			BuildScenesController();
            
//			BuildUISystems();
		}
	}
}