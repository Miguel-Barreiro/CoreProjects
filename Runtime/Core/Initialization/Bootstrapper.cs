using System;
using System.Collections.Generic;
using Core.Model;
using Core.Model.ModelSystems;
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
#if !UNITY_EDITOR
			try
			{
#endif
				SetupSystemsContainer();
				SetupObjectBuilder();
			
			
				List<SystemsInstallerBase> systemsInstallerBases = new List<SystemsInstallerBase>(Installers);
				Installers.Clear();

				//we need to have get a reference to type cache so it can be initialized before anything 
				TypeCache typeCache = TypeCache.Get();
				
				DataContainersControllerImplementation dataContainersController =
						DataContainersControllerImplementation.GetInstance();

				foreach (SystemsInstallerBase installer in systemsInstallerBases)
					installer.ResetComponentContainers(dataContainersController);
				
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
					installer.SetupConfigurations();
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
				
#if !UNITY_EDITOR
			} catch (Exception e)
			{
				Debug.LogError(e);
			}
#endif
			
			return OperationResult.Success();
		}


		#endregion


#region Internal


		private void SetupObjectBuilder()
		{
			ObjectBuilder objectBuilder;
				            
			if (!Container.HasBinding<ObjectBuilder>())
			{
				objectBuilder = ObjectBuilder.GetInstance();
						
				Container.BindInstance(objectBuilder);
			}
			else
			{
				objectBuilder = ObjectBuilder.GetInstance();
				// objectBuilder = Container.Resolve<ObjectBuilder>();
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
				EntitySystemsContainer entitySystemsContainer = new EntitySystemsContainer();
				Container.BindInstance(entitySystemsContainer);

				
				SystemsContainer systemsContainer = new SystemsContainer(entitySystemsContainer);
				Container.BindInstance(systemsContainer);
			}
		}

#endregion

	}
}