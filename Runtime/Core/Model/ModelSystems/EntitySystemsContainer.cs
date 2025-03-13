using System;
using System.Collections.Generic;
using System.Reflection;
using Core.Systems;
using UnityEngine;

namespace Core.Model
{
    public class EntitySystemsContainer
    {
        
        private readonly Dictionary<Type, SystemCache> systemByType = new();
        private readonly Dictionary<Type, SystemListenerGroup> systemsByComponentType = new ();
        
        #region Public
        
        internal IEnumerable<(Type, SystemListenerGroup)> GetAllComponentSystemsByComponentType()
        {
            foreach ((Type componentType, SystemListenerGroup group)  in systemsByComponentType)
            {
                yield return (componentType, group);
            }
        }
        
        
        internal IEnumerable<BaseEntitySystem> GetAllComponentSystems()
        {
            foreach (SystemCache systemCache in systemByType.Values)
            {
                yield return systemCache.System;
            }
        }

        
        internal IEnumerable<SystemCache> GetComponentSystemsFor(Type componentType, SystemPriority priority) {
            if (systemsByComponentType.TryGetValue(componentType, out SystemListenerGroup systems))
            {
                List<SystemCache> updatePriorityList = priority switch
                {
                    SystemPriority.Early => systems.EarlierPriority,
                    SystemPriority.Default => systems.DefaultPriority,
                    SystemPriority.Late => systems.LatePriority,
                    _ => systems.DefaultPriority
                };
                foreach (SystemCache systemCache in updatePriorityList)
                {
                    yield return systemCache;
                }
                //
                // foreach (SystemCache systemCache in systems.EarlierPriority)
                // {
                //     yield return systemCache;
                // }
                //
                // foreach (SystemCache systemCache in systems.DefaultPriority)
                // {
                //     yield return systemCache;
                // }
                //
                // foreach (SystemCache systemCache in systems.LatePriority)
                // {
                //     yield return systemCache;
                // }
            }
        }
        
        internal IEnumerable<SystemCache> GetComponentSystemsForDestroyed(Type componentType, SystemPriority priority) {
            if (systemsByComponentType.TryGetValue(componentType, out SystemListenerGroup systems))
            {
                List<SystemCache> updatePriorityList = priority switch
                {
                    SystemPriority.Early => systems.EarlierPriority,
                    SystemPriority.Default => systems.DefaultPriority,
                    SystemPriority.Late => systems.LatePriority,
                    _ => systems.DefaultPriority
                };
                foreach (SystemCache systemCache in updatePriorityList)
                {
                    yield return systemCache;
                }
                
                // foreach (SystemCache systemCache in systems.LatePriority)
                // {
                //     yield return systemCache;
                // }
                //
                // foreach (SystemCache systemCache in systems.DefaultPriority)
                // {
                //     yield return systemCache;
                // }
                //
                // foreach (SystemCache systemCache in systems.EarlierPriority)
                // {
                //     yield return systemCache;
                // }
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

            if (!systemsByComponentType.TryGetValue(system.EntityType, out SystemListenerGroup systemGroup))
            {
                systemGroup = new SystemListenerGroup();
                systemsByComponentType.Add(system.EntityType, systemGroup);
            }

            SystemCache systemCache = new SystemCache(system);
            systemGroup.Add(systemCache);
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

            if (!systemsByComponentType.TryGetValue(system.EntityType, out SystemListenerGroup systemGroup))
            {
                Debug.LogError($"System of type {systemType} not found in systemsByComponentType");
                return;
            }
            
            systemGroup.Remove(systemCache);
            systemByType.Remove(systemType);
        }

        #endregion
        
        public sealed class SystemListenerGroup
        {
            public readonly List<SystemCache> EarlierPriority = new ();
            public readonly List<SystemCache> DefaultPriority = new();
            public readonly List<SystemCache> LatePriority = new();

            public readonly List<SystemCache> UpdateEarlierPriority = new ();
            public readonly List<SystemCache> UpdateDefaultPriority = new();
            public readonly List<SystemCache> UpdateLatePriority = new();

            public void Add(SystemCache systemCache)
            {
                
                List<SystemCache> updatePriorityList = systemCache.SystemUpdatePriority switch
                {
                    SystemPriority.Early => UpdateEarlierPriority,
                    SystemPriority.Default => UpdateDefaultPriority,
                    SystemPriority.Late => UpdateLatePriority,
                    _ => UpdateDefaultPriority
                };
                List<SystemCache> lifetimePriorityList = systemCache.SystemLifetimePriority switch
                {
                    SystemPriority.Early => EarlierPriority,
                    SystemPriority.Default => DefaultPriority,
                    SystemPriority.Late => LatePriority,
                    _ => DefaultPriority
                };

                updatePriorityList.Add(systemCache);
                lifetimePriorityList.Add(systemCache);
            }
            
            public void Remove(SystemCache systemCache)
            {
                
                List<SystemCache> updatePriorityList = systemCache.SystemUpdatePriority switch
                {
                    SystemPriority.Early => UpdateEarlierPriority,
                    SystemPriority.Default =>UpdateDefaultPriority,
                    SystemPriority.Late => UpdateLatePriority,
                    _ => UpdateDefaultPriority
                };
                List<SystemCache> LifetimePriorityList = systemCache.SystemLifetimePriority switch
                {
                    SystemPriority.Early => EarlierPriority,
                    SystemPriority.Default => DefaultPriority,
                    SystemPriority.Late => LatePriority,
                    _ => DefaultPriority
                };

                updatePriorityList.Remove(systemCache);
                LifetimePriorityList.Remove(systemCache);
            }
        }
        
        public sealed class SystemCache
        {
            public readonly BaseEntitySystem System;
            public readonly Type CachedType;
            public readonly MethodInfo CachedOnNewEntityMethod;
            public readonly MethodInfo CachedOnEntityDestroyedMethod;
            public readonly MethodInfo CachedUpdateMethod;

            public readonly SystemPriority SystemUpdatePriority = SystemPriority.Default;
            public readonly SystemPriority SystemLifetimePriority = SystemPriority.Default;
            
            public SystemCache(BaseEntitySystem system)
            {
                System = system;

                Type systemType = system.GetType();

                Attribute[] attributes = Attribute.GetCustomAttributes(systemType);
                EntitySystemPropertiesAttribute systemProperties = GetOfType<EntitySystemPropertiesAttribute>(attributes);
                if (systemProperties != null)
                {
                    SystemLifetimePriority = systemProperties.LifetimePriority;
                    SystemUpdatePriority = systemProperties.LifetimePriority;
                }
                

                Type d1 = typeof(IModelSystem<>);
                Type generic = d1.MakeGenericType( system.EntityType );
                
                MethodInfo methodInfoOnNewEntityMethod = generic.GetMethod(nameof(IModelSystem<IEntity>.OnNew));
                MethodInfo methodInfoOnEntityDestroyedMethod = generic.GetMethod(nameof(IModelSystem<IEntity>.OnDestroy));
                MethodInfo methodInfoUpdateMethod = generic.GetMethod(nameof(IModelSystem<IEntity>.Update));
                
                CachedType = systemType;
                CachedOnNewEntityMethod = methodInfoOnNewEntityMethod;
                CachedOnEntityDestroyedMethod = methodInfoOnEntityDestroyedMethod;
                CachedUpdateMethod = methodInfoUpdateMethod;
            }
            
            private static T GetOfType<T>(Attribute[] attributes) where T : Attribute
            {
                foreach (Attribute attribute in attributes)
                {
                    if (attribute.GetType() == typeof(T))
                    {
                        return attribute as T;
                    }
                }

                return default;
            }

        }

    }
}