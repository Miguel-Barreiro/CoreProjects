using Core.Initialization;
using Core.Zenject.Source.Main;

namespace Core.Runtime.Editor
{
	internal class CoreSystemsForTestsInstaller : CoreSystemsInstaller
	{
		
		public CoreSystemsForTestsInstaller(DiContainer container) : base(container) { }
		
// 		protected override void InstallSystems()
// 		{
// 			base.InstallSystems();
// 			
//
// 			BuildEventManager();
// 			BuildGameLoopSystem();
//
// 			BuildEntityManager();
// 			BuildComponentSystemsLogic();
// 			BuildSystemsManager();
//
// 			//for tests we dont use these systems
// 			
// //			BuildViewSystems();
// //			BuildGenericGameobjePool();
// //			BuildScenesController();
//             
// //			BuildUISystems();
// 		}
	}
}