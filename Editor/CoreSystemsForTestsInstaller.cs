using Core.Initialization;
using Core.Model.ModelSystems;
using Core.Systems;
using Core.Zenject.Source.Main;

namespace Core.Editor
{

	public interface IUnitTestInstaller
	{
		public void AddTestSystem<T>(T system);
	}

	public class CoreSystemsForTestsInstaller : CoreSystemsInstaller, IUnitTestInstaller
	{
		private readonly UnitTest Test;

		public CoreSystemsForTestsInstaller(UnitTest test, DiContainer container) : base(container) { Test = test; }

		public override void SetupConfigurations() {  }

		protected override void InstallSystems()
		{
			base.InstallSystems();
			Test.AddUserSystems(this);
			SetSystemsControllerForTests();
		}

		public override void ResetComponentContainers(DataContainersController dataContainersController)
		{
			Test.ResetComponentContainersInternal(dataContainersController);
		}
		
		private void SetSystemsControllerForTests()
		{
			SystemsController systemsController = Container.Resolve<SystemsController>();
			systemsController.SetMode(SystemsController.SystemsControllerMode.UNIT_TESTS);
		}
		
		public void AddTestSystem<T>(T system)
		{
			BindInstance(system);
		}
	}
}