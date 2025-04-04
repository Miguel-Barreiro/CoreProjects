using System;
using System.Collections.Generic;
using Core.Systems;
using Core.Utils;
using Core.Utils.CachedDataStructures;
using Core.Zenject.Source.Main;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Initialization
{
	public sealed class Bootstrapper
	{
		private readonly DiContainer Container;
		
		private readonly List<SystemsInstallerBase> _currentSceneInstallers = new List<SystemsInstallerBase>();
		
		private readonly Queue<SystemsInstallerBase> Installers = new Queue<SystemsInstallerBase>();
		private readonly HashSet<SystemsInstallerBase> completedInstallers = new HashSet<SystemsInstallerBase>();


#region PUBLIC


		public Bootstrapper(DiContainer container) { Container = container; }
		// private readonly Dictionary<SystemsInstallerBase, bool> InstallersBy = new List<SystemsInstallerBase>();
		
		public void AddInstaller(SystemsInstallerBase installer, bool isSceneInstaller)
		{
			if (!completedInstallers.Contains(installer) && !Installers.Contains(installer))
			{
				Installers.Enqueue(installer);
				if (isSceneInstaller)
				{
					_currentSceneInstallers.Add(installer);
				}
			}
		}
		
		public IEnumerable<SystemsInstallerBase>  GetCurrentSceneInstallers()
		{
			return _currentSceneInstallers;
		}


		public void RemoveInstaller(SystemsInstallerBase installerToRemove)
		{
			_currentSceneInstallers.Remove(installerToRemove);
			completedInstallers.Remove(installerToRemove);
			if (Installers.Contains(installerToRemove))
			{
				using CachedList<SystemsInstallerBase> temp = ListCache<SystemsInstallerBase>.Get();
				while (Installers.TryDequeue(out SystemsInstallerBase installer))
				{
					if (installer != installerToRemove)
					{
						temp.Add(installer);
					}
				}
				foreach (SystemsInstallerBase installerToPuckBack in temp)
				{
					Installers.Enqueue(installerToPuckBack);
				}
			}
		}


		public async UniTask<OperationResult> Run()
		{
			try
			{

				SetupSystemsContainer();
				SetupObjectBuilder();
			
			
				List<SystemsInstallerBase> systemsInstallerBases = new List<SystemsInstallerBase>(Installers);
				Installers.Clear();
			
				foreach (SystemsInstallerBase installer in systemsInstallerBases)
				{
					installer.CreateSystems();
				}
			
				if(_currentSceneInstallers.Count > 0 )
				{
					foreach (SystemsInstallerBase currentSceneInstaller in _currentSceneInstallers)
					{
						UpdateObjectBuilder(currentSceneInstaller);
					}
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
					OperationResult result = await installer.LoadSystems();
					if (result.IsFailure)
						return result;
					completedInstallers.Add(installer);
				}

				foreach (SystemsInstallerBase installer in systemsInstallerBases)
				{
					installer.StartSystems();
				}

				foreach (SystemsInstallerBase installer in systemsInstallerBases)
				{
					installer.OnComplete();
				}
				
			} catch (Exception e)
			{
				Debug.LogError(e);
				throw;
			}
			
			return OperationResult.Success();
		}


		#endregion


#region Internal


		private void SetupObjectBuilder()
		{
			ObjectBuilder objectBuilder;
				            
			if (!Container.HasBinding<ObjectBuilder>())
			{
				objectBuilder = new ObjectBuilder();
						
				Container.BindInstance(objectBuilder);
			}
			else
			{
				objectBuilder = Container.Resolve<ObjectBuilder>();
			}
					
			Container.Inject(objectBuilder);
		}

		private void UpdateObjectBuilder(SystemsInstallerBase currentSceneInstaller)
		{
			ObjectBuilder objectBuilder = Container.Resolve<ObjectBuilder>();
			currentSceneInstaller.ContainerInstance.Inject(objectBuilder);
		}
		        
	
		        
		private void SetupSystemsContainer()
		{
			if (!Container.HasBinding<SystemsContainer>())
			{
				SystemsContainer systemsContainer = new SystemsContainer();
				Container.BindInstance(systemsContainer);
			}
		}

#endregion

	}
}