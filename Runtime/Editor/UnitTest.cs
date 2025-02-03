using Core.Initialization;
using Core.Runtime.Editor;
using Core.Zenject.Source.Main;
using NUnit.Framework;
using UnityEngine;

namespace Core.Editor
{
	public abstract class UnitTest : ScriptableObject
	{
		private DiContainer container;
		protected DiContainer Container => container;
		
		[OneTimeSetUp]
		public async void OneTimeSetUp()
		{
			container = new DiContainer();
			
			Bootstrapper bootstrapper = new Bootstrapper(container);
			bootstrapper.AddInstaller(new CoreSystemsForTestsInstaller(Container));

			bool result = await bootstrapper.Run();
		}

		
	}

}