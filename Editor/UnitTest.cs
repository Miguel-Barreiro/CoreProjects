using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Initialization;
using Core.Model.ModelSystems;
using Core.Systems;
using Core.Utils;
using Core.Utils.CachedDataStructures;
using Core.VSEngine;
using Core.VSEngine.Nodes.TestNodes;
using Core.Zenject.Source.Main;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Core.Editor
{
	public abstract class UnitTest : ScriptableObject
	{
		protected abstract void InstallTestSystems(IUnitTestInstaller installer);
		protected abstract void ResetComponentContainers(DataContainersController dataController);
		
		protected DiContainer Container => container;
		private DiContainer container;

		
		[Inject] private readonly SystemsController SystemsController = null!;


		internal void ResetComponentContainersInternal(DataContainersController dataController)
		{
			ResetComponentContainers(dataController);
		}

		internal void AddUserSystems(CoreSystemsForTestsInstaller installer)
		{
			InstallTestSystems(installer);
		}

		[OneTimeSetUp]
		public async Task OneTimeSetUp()
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
		
		protected void ExecuteTestNodes(ActionGraph actionGraph, VSEngineCore VSEngineCore)
		{

			using CachedList<TestStartNode> assertNodes = ListCache<TestStartNode>.Get();
			VSBaseEngine.GetTestStartNodes(actionGraph, assertNodes);

			foreach (TestStartNode testNode in assertNodes)
			{
				Debug.Log($"Running test node: {testNode.name}");
				for (int i = 0; i < testNode.RepeatNumber; i++)
					VSEngineCore.RunTestNode(testNode);
			}
		}


	}

}