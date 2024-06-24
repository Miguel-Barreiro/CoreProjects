using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Core.Model
{
    public class ComponentSystemsContainer
    {
        
        private readonly Dictionary<Type, SystemCache> systemByType = new();
        private readonly Dictionary<Type, List<SystemCache>> systemsByComponentType = new ();
        
        #region Public

        public void Init()
        {
            systemsByComponentType.Clear();
            systemByType.Clear();
        }
        
        internal IEnumerable<BaseComponentSystem> GetAllComponentSystems()
        {
            foreach (SystemCache systemCache in systemByType.Values)
            {
                yield return systemCache.System;
            }
        }

        
        internal IEnumerable<SystemCache> GetComponentSystemsFor(Type componentType) {
            if (systemsByComponentType.TryGetValue(componentType, out List<SystemCache> systems))
            {
                foreach (SystemCache systemCache in systems)
                {
                    yield return systemCache;
                }
            }
        }

        internal void AddComponentSystem(BaseComponentSystem system) 
        {
            Type systemType = system.GetType();
            if (systemByType.ContainsKey(systemType))
            {
                Debug.LogError($"System of type {systemType} already added");
                return;
            }

            if (!systemsByComponentType.TryGetValue(system.ComponentType, out List<SystemCache> systemList))
            {
                systemList = new List<SystemCache>();
                systemsByComponentType.Add(system.ComponentType, systemList);
            }

            SystemCache systemCache = new SystemCache(system);
            systemList.Add(systemCache);
            systemByType.Add(systemType, systemCache);
        }
        
        internal void RemoveComponentSystem(BaseComponentSystem system)
        {
            Type systemType = system.GetType();
            if (!systemByType.TryGetValue(systemType, out SystemCache systemCache))
            {
                Debug.LogError($"System of type {systemType} not found");
                return;
            }

            if (!systemsByComponentType.TryGetValue(system.ComponentType, out List<SystemCache> systemList))
            {
                Debug.LogError($"System of type {systemType} not found in systemsByComponentType");
                return;
            }

            systemList.Remove(systemCache);
            systemByType.Remove(systemType);
        }

        #endregion
        
        public sealed class SystemCache
        {
            public readonly BaseComponentSystem System;
            public readonly Type CachedType;
            public readonly MethodInfo CachedOnNewEntityMethod;
            public readonly MethodInfo CachedOnEntityDestroyedMethod;
            public readonly MethodInfo CachedUpdateMethod;

            public SystemCache(BaseComponentSystem system)
            {
                System = system;
                
                Type d1 = typeof(ComponentSystem<>);
                Type generic = d1.MakeGenericType( system.ComponentType );
                
                MethodInfo methodInfoOnNewEntityMethod = generic.GetMethod(nameof(ComponentSystem<IComponent>.OnNewComponent));
                MethodInfo methodInfoOnEntityDestroyedMethod = generic.GetMethod(nameof(ComponentSystem<IComponent>.OnComponentDestroy));
                MethodInfo methodInfoUpdateMethod = generic.GetMethod(nameof(ComponentSystem<IComponent>.UpdateComponent));
                        
                CachedType = generic;
                CachedOnNewEntityMethod = methodInfoOnNewEntityMethod;
                CachedOnEntityDestroyedMethod = methodInfoOnEntityDestroyedMethod;
                CachedUpdateMethod = methodInfoUpdateMethod;
            }

        }

    }
}