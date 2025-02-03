using System.Collections.Generic;
using Core.Systems;
using Core.Zenject.Source.Main;
using Cysharp.Threading.Tasks;

namespace Core.Initialization
{
	public sealed class Bootstrapper
	{
		// private readonly DiContainer Container;
		
		private readonly Queue<SystemsInstallerBase> Installers = new Queue<SystemsInstallerBase>();
		private readonly HashSet<SystemsInstallerBase> completedInstallers = new HashSet<SystemsInstallerBase>();


#region PUBLIC


		// public Bootstrapper(DiContainer container) { Container = container; }
		// private readonly Dictionary<SystemsInstallerBase, bool> InstallersBy = new List<SystemsInstallerBase>();
		
		public void AddInstaller(SystemsInstallerBase installer)
		{
			if (!completedInstallers.Contains(installer) && !Installers.Contains(installer))
			{
				Installers.Enqueue(installer);
			}
		}

		internal async UniTask<bool> Run()
		{
			
			List<SystemsInstallerBase> systemsInstallerBases = new List<SystemsInstallerBase>(Installers);
			Installers.Clear();
			
			foreach (SystemsInstallerBase installer in systemsInstallerBases)
			{
				installer.CreateSystems();
			}
			
			foreach (SystemsInstallerBase installer in systemsInstallerBases)
			{
				installer.InjectInstances();
			}
			
			foreach (SystemsInstallerBase installer in systemsInstallerBases)
			{
				installer.InitializeInstances();
			}
			
			foreach (SystemsInstallerBase installer in systemsInstallerBases)
			{
				bool result = await installer.LoadSystems();
				if (result)
				{
					completedInstallers.Add(installer);
				} else
				{
					return false;
				}
			}

			foreach (SystemsInstallerBase installer in systemsInstallerBases)
			{
				installer.StartSystems();
			}

			foreach (SystemsInstallerBase installer in systemsInstallerBases)
			{
				installer.OnComplete();
			}
			
			return true;
		}
		

#endregion



	}
}