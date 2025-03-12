using Core.Initialization;
using Core.Systems;
using Core.Utils;
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
			bootstrapper.AddInstaller(installer, false);

			OperationResult result = await bootstrapper.Run();
			if (result.IsFailure)
			{
				Debug.LogError("Failed to run test bootstrapper");
				return;
			}

			Container.Inject(this);
		}
		
		protected void ExecuteFrame(float time)
		{
			SystemsController.ExecuteFrame(time);
		}

		protected void ExecuteFrameMs(float time)
		{
			SystemsController.ExecuteFrame(time/1000.0f);
		}

	}

}