using Core.Initialization;
using Core.Model;
using Core.Systems;
using Core.Zenject.Source.Main;
using Testing_Core.Model.DataDrivenTests;

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
		
		protected override void InstallSystems()
		{
			base.InstallSystems();
			Test.AddUserSystems(this);
			SetSystemsControllerForTests();
		}

		public override void ResetComponentContainers()
		{
			ComponentContainersController.ResetContainer<TestDD1Data>(10);
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