using Core.Initialization;
using Core.Systems;
using Core.Zenject.Source.Main;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Core.Editor
{
	public abstract class UnitTest : ScriptableObject
	{
		protected abstract void InstallTestSystems(IUnitTestInstaller installer);
		
		protected DiContainer Container => container;
		private DiContainer container;

		
		[Inject] private readonly SystemsController SystemsController = null!;

		internal void AddUserSystems(CoreSystemsForTestsInstaller installer)
		{
			InstallTestSystems(installer);
		}

		[OneTimeSetUp]
		public async void OneTimeSetUp()
		{
			container = new DiContainer();

			CoreSystemsForTestsInstaller installer = new CoreSystemsForTestsInstaller(this, Container);

			Bootstrapper bootstrapper = new Bootstrapper(container);
			bootstrapper.AddInstaller(installer);
			
			bool result = await bootstrapper.Run();
			
			Container.Inject(this);
		}
		
		protected void ExecuteFrame(float time)
		{
			SystemsController.ExecuteFrame(time);
		}


	}

}