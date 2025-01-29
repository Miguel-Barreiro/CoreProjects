using Core.Model;
using UnityEngine;

namespace Core.Initialization
{
	internal class CoreSystemsForTestsInstaller : CoreSystemsInstaller
	{
		public CoreSystemsForTestsInstaller(Transform rootCoreParent) : base(rootCoreParent) { }
		
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