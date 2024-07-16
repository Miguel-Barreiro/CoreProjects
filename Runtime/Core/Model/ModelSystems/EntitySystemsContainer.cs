using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Core.Model
{
    public class EntitySystemsContainer
    {
        
        private readonly Dictionary<Type, SystemCache> systemByType = new();
        private readonly Dictionary<Type, List<SystemCache>> systemsByComponentType = new ();
        
        #region Public
        
        internal IEnumerable<(Type, List<SystemCache>)> GetAllComponentSystemsByComponentType()
        {
            foreach ((Type componentType, List<SystemCache> systemCaches)  in systemsByComponentType)
            {
                yield return (componentType, systemCaches);
            }
        }
        
        
        internal IEnumerable<BaseEntitySystem> GetAllComponentSystems()
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

        internal void AddComponentSystem(BaseEntitySystem system) 
        {
            Type systemType = system.GetType();
            if (systemByType.ContainsKey(systemType))
            {
                Debug.LogError($"Model System of type {systemType} already added");
                return;
            }

            if (!systemsByComponentType.TryGetValue(system.EntityType, out List<SystemCache> systemList))
            {
                systemList = new List<SystemCache>();
                systemsByComponentType.Add(system.EntityType, systemList);
            }

            SystemCache systemCache = new SystemCache(system);
            systemList.Add(systemCache);
            systemByType.Add(systemType, systemCache);
        }
        
        internal void RemoveComponentSystem(BaseEntitySystem system)
        {
            Type systemType = system.GetType();
            if (!systemByType.TryGetValue(systemType, out SystemCache systemCache))
            {
                Debug.LogError($"System of type {systemType} not found");
                return;
            }

            if (!systemsByComponentType.TryGetValue(system.EntityType, out List<SystemCache> systemList))
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
            public readonly BaseEntitySystem System;
            public readonly Type CachedType;
            public readonly MethodInfo CachedOnNewEntityMethod;
            public readonly MethodInfo CachedOnEntityDestroyedMethod;
            public readonly MethodInfo CachedUpdateMethod;

            public SystemCache(BaseEntitySystem system)
            {
                System = system;
                
                Type d1 = typeof(IModelSystem<>);
                Type generic = d1.MakeGenericType( system.EntityType );
                
                MethodInfo methodInfoOnNewEntityMethod = generic.GetMethod(nameof(IModelSystem<IEntity>.OnNew));
                MethodInfo methodInfoOnEntityDestroyedMethod = generic.GetMethod(nameof(IModelSystem<IEntity>.OnDestroy));
                MethodInfo methodInfoUpdateMethod = generic.GetMethod(nameof(IModelSystem<IEntity>.Update));
                
                CachedType = generic;
                CachedOnNewEntityMethod = methodInfoOnNewEntityMethod;
                CachedOnEntityDestroyedMethod = methodInfoOnEntityDestroyedMethod;
                CachedUpdateMethod = methodInfoUpdateMethod;
            }

        }

    }
}