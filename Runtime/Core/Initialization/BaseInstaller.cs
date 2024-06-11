using System;
using System.Collections.Generic;
using Core.Systems;
using Core.Utils.CachedDataStructures;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using Object = System.Object;

namespace Core.Initialization
{
    public abstract class BaseInstaller : MonoInstaller, IDisposable
    {
        public abstract bool InstallComplete { get; }
        
        public bool LoadedComplete => loadedSystems;
        public bool StartedComplete => startedSystems;
        
        protected abstract void Instantiate();
        protected virtual void InstantiateInternalSystems() { } 

        public override async void InstallBindings()
        {
            UpdateSystemsContainer();
            
            gameObject.SetActive(true);
            Clear();

            InstantiateInternalSystems();
            UpdateObjectBuilder();

            Instantiate();
            InjectInstances();
            
            InitializeInstances();

            OnComplete();
        }


        private RunnableContext runnableContext;
        private void Awake()
        {
            runnableContext = GetComponent<RunnableContext>();
        }

        public void Dispose()
        {
            Clear();
        }

        private void OnDestroy()
        {
            Clear();
        }

        protected virtual void OnComplete() { }

        #region System Installation

        protected GameObject InstantiateAndBindPrefab<T>(T prefab, Transform parent = null) where T : MonoBehaviour
        {
            GameObject logicInstance = Instantiate(prefab.gameObject, parent);
            BindComponentInHierarchy<T>(logicInstance);
            return logicInstance;
        }		
		
        protected GameObject InstantiatePrefab(GameObject prefab, Transform parent = null) 
        {
            GameObject logicInstance = Instantiate(prefab, parent);
            return logicInstance;
        }

        protected void BindInstance<T>(T instance)
        {
            RegisterDisposableIfNeeded(instance);
            Container.BindInstance<T>(instance);
            AddSystem(instance);
        }
		
		
        protected T InstantiateAndBindType<T>()
        {
            T newSystem = Container.Instantiate<T>();
            RegisterDisposableIfNeeded(newSystem);
            Container.BindInstance<T>(newSystem);
            AddSystem(newSystem);
            return newSystem;
        }

        protected void BindComponentInHierarchy<T>(GameObject instance)
        {
            T component = instance.GetComponent<T>();
            if(component == null)
            {
                component = instance.GetComponentInChildren<T>();
            }
            if(component == null)
            {
                Debug.LogError($"ERROR: {typeof(T)} not found in {instance.name}");
                return;
            }
            Container.BindInterfacesAndSelfTo<T>().FromInstance(component).AsSingle();
            AddSystem(component);
        }

        protected void BindComponentFromSelf<T>(GameObject instance)
        {
            T component = instance.GetComponent<T>();
            Container.BindInterfacesAndSelfTo<T>().FromInstance(component).AsSingle();
            AddSystem(component);
        }
		
        protected void BindComponent<T>(T component) where T : MonoBehaviour
        {
            Container.BindInterfacesAndSelfTo<T>().FromInstance(component).AsSingle();
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
        
        
        public async UniTask LoadSystems()
        {
            if (loadedSystems)
            {
                return;
            }
            
            List<UniTask> loadTasks = new (10);
            foreach (Object system in ownedSystems)
            {
                if(system is ILoadSystem loadSystem)
                {
                    UniTask<bool> uniTask = loadSystem.Load(out Action retryAction);
                    loadTasks.Add(uniTask);
                }
            }
            
            await UniTask.WhenAll(loadTasks);
            loadedSystems = true;
        }

        public void StartSystems()
        {
            if (startedSystems)
            {
                return;
            }
            
            foreach (Object system in ownedSystems)
            {
                if(system is IStartSystem startSystem)
                {
                    startSystem.Start();
                }
            }
            
            startedSystems = true;
        }


#endregion
        
   
#region Internal

        protected readonly Dictionary<Type, List<IDisposable>> disposableBindedTypes = new Dictionary<Type, List<IDisposable>>();
        protected readonly List<GameObject> injectableGameObjects = new ();
        protected readonly List<System.Object> injectableObjects = new ();

        protected readonly List<Object> ownedSystems = new ();
        protected readonly List<GameObject> ownedGameObjectSystems = new ();

        protected bool loadedSystems = false;
        protected bool startedSystems = false;
        
        
        protected RunnableContext RunnableContext => runnableContext;
        
        protected void Clear()
        {
            injectableGameObjects.Clear();
            injectableObjects.Clear();

            SystemsContainer systemsContainer = Container.Resolve<SystemsContainer>();
            
            foreach((Type _, List<IDisposable> disposables) in disposableBindedTypes)
            {
                foreach (IDisposable disposable in disposables)
                {
                    systemsContainer.RemoveSystem(disposable);
                    disposable.Dispose();
                }
            }
            disposableBindedTypes.Clear();
            
            using CachedList<Object> ownedSystemsTemp = ListCache<Object>.Get();
            ownedSystemsTemp.AddRange(this.ownedSystems);
            foreach (Object system in ownedSystemsTemp)
            {
                RemoveSystem(system);
            }
            ownedSystems.Clear();
            ownedGameObjectSystems.Clear();
        }

        #region Injection

        private void UpdateSystemsContainer()
        {
            if (!Container.HasBinding<SystemsContainer>())
            {
                SystemsContainer systemsContainer = new SystemsContainer();
                Container.BindInstance(systemsContainer);
            }
        }

        private void UpdateObjectBuilder()
        {
            ObjectBuilder objectBuilder;
            
            if (!Container.HasBinding<ObjectBuilder>())
            {
                objectBuilder = new ObjectBuilder();
                BindInstance(objectBuilder);
            }
            else
            {
                objectBuilder = Container.Resolve<ObjectBuilder>();
            }

            AddInjectable(objectBuilder);
        }

        
        protected void AddInjectable(GameObject logicInstance)
        {
            if (!injectableGameObjects.Contains(logicInstance))
            {
                injectableGameObjects.Add(logicInstance);	
            }
        }

        
        protected void AddInjectable(System.Object logicInstance)
        {
            if (!injectableObjects.Contains(logicInstance))
            {
                injectableObjects.Add(logicInstance);	
            }
        }

        
        #endregion

        #region Flow

        private void InitializeInstances()
        {
            foreach (Object system in ownedSystems)
            {
                if(system is IInitSystem initSystem)
                {
                    initSystem.Initialize();
                }
            }
        }

        
        private void InjectInstances()
        {
            foreach (Object injectableObject in injectableObjects)
            {
                Container.Inject(injectableObject);
            }
        }

        
        private void AddSystem<T>(T logicInstance)
        {
            AddInjectable(logicInstance);

            SystemsContainer systemsContainer = Container.Resolve<SystemsContainer>();
            systemsContainer.AddSystem(logicInstance);
            ownedSystems.Add(logicInstance);
        }
        
        private void AddSystemsFromGameobject(GameObject logicInstance)
        {
            Component[] componentsInChildren = logicInstance.GetComponentsInChildren<Component>();
            foreach (Component component in componentsInChildren)
            {
                AddSystem(component);
            }
            ownedGameObjectSystems.Add(logicInstance);
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
