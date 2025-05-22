using System;
using System.Collections.Generic;
using Core.Model.ModelSystems;
using Core.Systems;
using Core.Utils;
using Core.Utils.CachedDataStructures;
using Core.View.UI;
using Core.Zenject.Source.Main;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Initialization
{
    
    public abstract class SystemsInstallerBase
    {
        public abstract void SetupConfigurations();
        protected abstract void InstallSystems();
        public abstract void ResetComponentContainers(DataContainersController dataController);

        protected readonly DiContainer Container;
        internal DiContainer ContainerInstance => Container;
        
        internal virtual void OnComplete() { }

        internal void CreateSystems()
        {
            InstallSystems();
        }
        
        internal void UninstallSystems()
        {
            Clear();
        }
        
        protected T GetSystem<T>()
        {
            return Container.Resolve<T>();
        }

        internal void InitializeInstances()
        {
            foreach ( (System.Object system, List<Type> types) in ownedSystems)
            {
                if(system is IInitSystem initSystem)
                {
                    initSystem.Initialize();
                }
            }
        }

        internal void InjectInstances()
        {
            foreach (System.Object injectableObject in injectableObjects)
            {
                Container.Inject(injectableObject);
            }
        }

        private const string FAIL_LOAD_MESSAGE = "Failed to load systems";
        internal async UniTask<OperationResult> LoadSystems()
        {
            List<UniTask<bool>> loadTasks = new (10);
            foreach ( (System.Object system, List<Type> types) in ownedSystems)
            {
                if(system is ILoadSystem loadSystem)
                {
                    UniTask<bool> uniTask = loadSystem.Load(out Action retryAction);
                    loadTasks.Add(uniTask);
                }
            }
            
            await UniTask.WhenAll(loadTasks);

            bool result = true;
            foreach (UniTask<bool> loadTask in loadTasks)
            {
                result = result && loadTask.AsValueTask().Result;
            }
            return result? OperationResult.Success() : OperationResult.Failure(FAIL_LOAD_MESSAGE);
        }

        internal void StartSystems()
        {
            foreach ( (System.Object system, List<Type> types) in ownedSystems)
            {
                if(system is IStartSystem startSystem)
                {
                    startSystem.StartSystem();
                }
            }
        }

        
#region System Installation

        protected GameObject InstantiateAndBindPrefab<T>(T prefab, Transform parent = null) where T : MonoBehaviour
        {
            GameObject logicInstance = GameObject.Instantiate(prefab.gameObject, parent);
            BindComponentInHierarchy<T>(logicInstance);
            return logicInstance;
        }		
		
        protected GameObject InstantiatePrefab(GameObject prefab, Transform parent = null) 
        {
            GameObject logicInstance = GameObject.Instantiate(prefab, parent);
            return logicInstance;
        }

        protected void BindInstance<T>(T instance)
        {
            RegisterDisposableIfNeeded(instance);
            Container.BindInstance<T>(instance);
            AddSystem(instance);
        }
        
        protected void BindInstanceByDynamicType(object instance, Type type)
        {
            RegisterDisposableIfNeeded(instance);
            Container.Bind(type).FromInstance(instance);
            AddSystem(instance);
        }

        public T InstallSystems<T>()
        {
            T newSystem = Container.Instantiate<T>();
            RegisterDisposableIfNeeded(newSystem);
            Container.BindInstance<T>(newSystem);
            AddSystem(newSystem);
            return newSystem;
        }


        protected void RegisterUIScreenDefinition<TMessenger>( UIScreenDefinition definition, TMessenger messenger)
            where TMessenger : UIMessenger
        {
            if (definition == null)
            {
                Debug.LogError($"RegisterUIScreenDefinition: UIScreenDefinition is null"); 
                return;
            }

            if (messenger == null)
            {
                Debug.LogError($"RegisterUIScreenDefinition: messenger is null"); 
                return;
            }

            
            UIRootImplementation uiRoot = Container.Resolve<UIRootImplementation>();
            uiRoot.Register(definition, messenger);
        }

        
        protected void RegisterUIScreenDefinition<TMessenger>( UIScreenDefinition definition, UIView<TMessenger> view, TMessenger messenger)
            where TMessenger : UIMessenger
        {
            
            UIRootImplementation uiRoot = Container.Resolve<UIRootImplementation>();
            uiRoot.RegisterWithViewObject(definition, messenger, view);
        }



        protected void BindComponentInHierarchy<T>(GameObject instance)
        {
            T component = instance.GetComponent<T>();
            if(component == null)
            {
                component = instance.GetComponentInChildren<T>(true);
            }
            if(component == null)
            {
                Debug.LogError($"ERROR: {typeof(T)} not found in {instance.name}");
                return;
            }
            Container.BindInstance<T>(component);
            // Container.BindInterfacesAndSelfTo<T>().FromInstance(component).AsSingle();
            AddSystem(component);
        }

        protected void BindComponentFromSelf<T>(GameObject instance)
        {
            T component = instance.GetComponent<T>();
            Container.BindInstance<T>(component);
            // Container.BindInterfacesAndSelfTo<T>().FromInstance(component).AsSingle();
            AddSystem(component);
        }
		
        protected void BindComponent<T>(T component) where T : MonoBehaviour
        {
            Container.BindInstance<T>(component);
            // Container.BindInterfacesAndSelfTo<T>().FromInstance(component).AsSingle();
            AddSystem(component);
        }
        
        protected void ClearDisposablesIfNeeded<T>()
        {
            SystemsContainer systemsContainer = Container.Resolve<SystemsContainer>();
            
            if (disposableBindedTypes.TryGetValue(typeof(T), out List<IDisposable> thisTypeList))
            {
                foreach (IDisposable disposable in thisTypeList)
                {
                    systemsContainer.RemoveSystem(disposable);
                    disposable.Dispose();
                }
                thisTypeList.Clear();
            }
        }
		
        protected void Unbind<T>()
        {
            ClearDisposablesIfNeeded<T>();
            Container.Unbind<T>();
        }
        
        


#endregion
        
   
#region Internal

        protected readonly Dictionary<Type, List<IDisposable>> disposableBindedTypes = new Dictionary<Type, List<IDisposable>>();
        protected readonly List<GameObject> injectableGameObjects = new ();
        protected readonly List<System.Object> injectableObjects = new ();

        protected readonly Dictionary<System.Object, List<Type>> ownedSystems = new ();
        protected readonly List<GameObject> ownedGameObjectSystems = new ();
        protected SystemsInstallerBase(DiContainer container) { Container = container; }

        protected void Clear()
        {
            using CachedList<System.Object> ownedSystemsTemp = ListCache<System.Object>.Get();
            foreach ( (System.Object system, List<Type> types) in ownedSystems)
            {
                ownedSystemsTemp.Add(system);
                foreach (Type type in types)
                    Container.Unbind(type);
            }
            foreach (GameObject system in ownedGameObjectSystems)
            {
                GameObject.Destroy(system);
            }
            
            foreach (System.Object system in ownedSystemsTemp)
            {
                RemoveSystem(system);
            }
            ownedSystems.Clear();
            ownedGameObjectSystems.Clear();
        }

        #region Injection
        
        protected void AddInjectable(GameObject logicInstance)
        {
            if (!injectableGameObjects.Contains(logicInstance))
            {
                injectableGameObjects.Add(logicInstance);	
            }
        }


        private void AddInjectable(System.Object logicInstance)
        {
            if (!injectableObjects.Contains(logicInstance))
            {
                injectableObjects.Add(logicInstance);	
            }
        }

        
        #endregion

        #region Flow
        
        
        private void AddSystem<T>(T logicInstance)
        {
            if (logicInstance == null)
            {
                Debug.LogError($"Installing System {typeof(T)} but was given null"); 
                return;
            }
            RegisterDisposableIfNeeded(logicInstance);
            AddInjectable(logicInstance);
            
            if (!ownedSystems.ContainsKey(logicInstance))
            {
                SystemsContainer systemsContainer = Container.Resolve<SystemsContainer>();
                systemsContainer.AddSystem(logicInstance, this.GetType().Name);
                ownedSystems.Add(logicInstance, new());
            }
            ownedSystems[logicInstance].Add(typeof(T));
        }
        
        private void AddSystemsFromGameobject(GameObject logicInstance)
        {
            if (!ownedGameObjectSystems.Contains(logicInstance))
            {
                Component[] componentsInChildren = logicInstance.GetComponentsInChildren<Component>();
                foreach (Component component in componentsInChildren)
                {
                    AddSystem(component);
                }
                ownedGameObjectSystems.Add(logicInstance);
            }

        }
        private void RemoveSystem<T>(T logicInstance)
        {
            SystemsContainer systemsContainer = Container.Resolve<SystemsContainer>();
            systemsContainer.RemoveSystem(logicInstance);
            ownedSystems.Remove(logicInstance);
        }

        private void RemoveSystemsFromGameobject(GameObject logicInstance)
        {
            Component[] componentsInChildren = logicInstance.GetComponentsInChildren<Component>();
            foreach (Component component in componentsInChildren)
            {
                RemoveSystem(component);
            }
            ownedGameObjectSystems.Remove(logicInstance);
        }
        
        protected void RegisterDisposableIfNeeded<T>(T instance)
        {
            if(!(instance is IDisposable disposableInstance))
            {
                return;
            }

            if(!disposableBindedTypes.TryGetValue(typeof(T), out List<IDisposable> thisTypeList))
            {
                thisTypeList = new List<IDisposable>();
                disposableBindedTypes[typeof(T)] = thisTypeList;
            }

            if (!thisTypeList.Contains(disposableInstance))
            {
                thisTypeList.Add(disposableInstance);
            }
        }

        #endregion
        
        
#endregion

    }
}