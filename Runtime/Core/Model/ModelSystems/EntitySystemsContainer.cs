using System;
using System.Collections.Generic;
using System.Linq;
using Core.Model.ModelSystems.ComponentSystems;
using Core.Systems;
using UnityEngine;

namespace Core.Model.ModelSystems
{

    // internal interface EntitySystemsContainer
    // {
    //
    //     // internal ComponentSystemListenerGroup GetAllComponentSystemsFor(Type componentType);
    //     // internal IEnumerable<UpdateComponentSystemCache> GetComponentSystemsForUpdate(Type componentType, SystemPriority priority);
    //     internal IEnumerable<(Type, ComponentSystemListenerGroup)> GetAllComponentSystemsByComponentType();
    //
    //     internal void AddSystem(object system);
    //     internal void RemoveComponentSystem(object system);
    //     
    // }
    
    




    public class EntitySystemsContainer
    {

        private readonly List<Type> EarlyComponentData= new List<Type>();
        private readonly List<Type> DefaultComponentData= new List<Type>();
        private readonly List<Type> LateComponentData= new List<Type>();
        
        private readonly Dictionary<Type, ComponentSystemListenerGroup> SystemsCacheByComponentDataType = new ();
        private readonly List<object> systems = new();


        internal EntitySystemsContainer()
        {
            foreach (var componentDataType in TypeCache.Get().GetAllComponentDataTypes())
            {
                SystemsCacheByComponentDataType.Add(componentDataType, new ComponentSystemListenerGroup());
    
                Attribute[] attributes = Attribute.GetCustomAttributes(componentDataType);
                ComponentDataPropertiesAttribute systemProperties = GetAttributesOfType<ComponentDataPropertiesAttribute>(attributes);
                SystemPriority systemPriority = systemProperties?.Priority ?? SystemPriority.Default;

                List<Type> target = systemPriority switch
                {
                    SystemPriority.Early => EarlyComponentData,
                    SystemPriority.Default => DefaultComponentData,
                    SystemPriority.Late => LateComponentData,
                    _ => DefaultComponentData
                };
                target.Add(componentDataType);
            }
            
            T GetAttributesOfType<T>(Attribute[] attributes) where T : Attribute
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
        
        
        
        #region Public
        
        // internal IEnumerable<(Type, ComponentSystemListenerGroup)> GetAllComponentSystemsByComponentType()
        // {
        //     foreach ((Type componentType, ComponentSystemListenerGroup group)  in systemsByComponentType)
        //     {
        //         yield return (componentType, group);
        //     }
        // }
        
        
        // internal IEnumerable<UpdateComponentSystemCache> GetComponentSystemsForUpdate(Type componentType, SystemPriority priority) {
        //     if (systemsCacheByComponentType.TryGetValue(componentType, out ComponentSystemListenerGroup systems))
        //     {
        //         List<UpdateComponentSystemCache> priorityList = priority switch
        //         {
        //             SystemPriority.Early => systems.UpdateEarlierPriority,
        //             SystemPriority.Default => systems.UpdateDefaultPriority,
        //             SystemPriority.Late => systems.UpdateLatePriority,
        //             _ => systems.UpdateDefaultPriority
        //         };
        //         foreach (UpdateComponentSystemCache systemCache in priorityList)
        //         {
        //             yield return systemCache;
        //         }
        //     }
        // }
        //
        // internal IEnumerable<OnDestroyComponentSystemCache> GetComponentSystemsForDestroyed(Type componentType, SystemPriority priority) {
        //     if (systemsCacheByComponentType.TryGetValue(componentType, out ComponentSystemListenerGroup systems))
        //     {
        //         List<OnDestroyComponentSystemCache> priorityList = priority switch
        //         {
        //             SystemPriority.Early => systems.OnDestroyEarlierPriority,
        //             SystemPriority.Default => systems.OnDestroyDefaultPriority,
        //             SystemPriority.Late => systems.OnDestroyLatePriority,
        //             _ => systems.OnDestroyDefaultPriority
        //         };
        //         foreach (OnDestroyComponentSystemCache systemCache in priorityList)
        //         {
        //             yield return systemCache;
        //         }
        //     }
        // }
        //
        // internal IEnumerable<OnCreateComponentSystemCache> GetComponentSystemsForCreated(Type componentType, SystemPriority priority) {
        //     if (systemsCacheByComponentType.TryGetValue(componentType, out ComponentSystemListenerGroup systems))
        //     {
        //         List<OnCreateComponentSystemCache> priorityList = priority switch
        //         {
        //             SystemPriority.Early => systems.OnCreateEarlierPriority,
        //             SystemPriority.Default => systems.OnCreateDefaultPriority,
        //             SystemPriority.Late => systems.OnCreateLatePriority,
        //             _ => systems.OnCreateDefaultPriority
        //         };
        //         foreach (OnCreateComponentSystemCache systemCache in priorityList)
        //         {
        //             yield return systemCache;
        //         }
        //     }
        // }

        // internal ComponentSystemListenerGroup GetAllComponentSystemsFor(Type componentType)
        // {
        //     if (!systemsCacheByComponentType.TryGetValue(componentType, out ComponentSystemListenerGroup systems))
        //     {
        //         Debug.LogError($"No systems found for component type {componentType}");
        //         return null;
        //     }
        //     return systems;
        // }

        internal IEnumerable<(Type, ComponentSystemListenerGroup)> GetAllComponentSystemsByComponentDataType()
        {
            foreach (Type componentData in EarlyComponentData)
            {
                yield return (componentData, SystemsCacheByComponentDataType[componentData]);
            }
            foreach (Type componentData in DefaultComponentData)
            {
                yield return (componentData, SystemsCacheByComponentDataType[componentData]);
            }
            foreach (Type componentData in LateComponentData)
            {
                yield return (componentData, SystemsCacheByComponentDataType[componentData]);
            }
            //
            // foreach (var kvp in SystemsCacheByComponentDataType)
            // {
            //     yield return kvp;
            // }
            // return SystemsCacheByComponentDataType.AsEnumerable();
        }

        internal void AddSystem(object system) 
        {
            Type systemType = system.GetType();
            if (systems.Contains(system))
            {
                Debug.LogError($"Model System of type {systemType} already added");
                return;
            }

            IEnumerable<Type> componentDatas = ComponentUtils.GetAllComponentDataTypesFromSystem(systemType);
            foreach (Type componentDataType in componentDatas)
            {
                ComponentSystemListenerGroup componentSystemListenerGroup = SystemsCacheByComponentDataType[componentDataType];
                componentSystemListenerGroup.AddSystem(system, componentDataType);
            }
        }

        internal void RemoveComponentSystem(object system)
        {
            Type systemType = system.GetType();
            if (!systems.Contains(system)) return;

            IEnumerable<Type> components = ComponentUtils.GetAllComponentDataTypesFromSystem(systemType);
            foreach (Type componentType in components)
            {
                ComponentSystemListenerGroup componentSystemListenerGroup = SystemsCacheByComponentDataType[componentType];
                componentSystemListenerGroup.RemoveSystem(system);
            }
        }

        #endregion
        
        

    }
    public sealed class ComponentSystemListenerGroup
    {
        public readonly List<OnCreateComponentSystemCache> OnCreateEarlierPriority = new ();
        public readonly List<OnCreateComponentSystemCache> OnCreateDefaultPriority = new();
        public readonly List<OnCreateComponentSystemCache> OnCreateLatePriority = new();

        public readonly List<OnDestroyComponentSystemCache> OnDestroyEarlierPriority = new ();
        public readonly List<OnDestroyComponentSystemCache> OnDestroyDefaultPriority = new();
        public readonly List<OnDestroyComponentSystemCache> OnDestroyLatePriority = new();
        
        public readonly List<UpdateComponentSystemCache> UpdateEarlierPriority = new ();
        public readonly List<UpdateComponentSystemCache> UpdateDefaultPriority = new();
        public readonly List<UpdateComponentSystemCache> UpdateLatePriority = new();

        
        public void AddSystem(object system, Type componentType)
        {
            Add(OnCreateComponentSystemCache.CreateIfPossible(system, componentType));
            Add(OnDestroyComponentSystemCache.CreateIfPossible(system, componentType));
            Add(UpdateComponentSystemCache.CreateIfPossible(system, componentType));
        }

        public void RemoveSystem(object system)
        {
            OnCreateEarlierPriority.RemoveAll(cache => cache.System == system );
            OnCreateDefaultPriority.RemoveAll(cache => cache.System == system );
            OnCreateLatePriority.RemoveAll(cache => cache.System == system );
            OnDestroyEarlierPriority.RemoveAll(cache => cache.System == system );
            OnDestroyDefaultPriority.RemoveAll(cache => cache.System == system );
            OnDestroyLatePriority.RemoveAll(cache => cache.System == system );
            UpdateEarlierPriority.RemoveAll(cache => cache.System == system );
            UpdateDefaultPriority.RemoveAll(cache => cache.System == system );
            UpdateLatePriority.RemoveAll(cache => cache.System == system );
        }
        
        private void Add(BaseSystemCache systemCache)
        {
            if(systemCache== null) return;

            if (systemCache is OnDestroyComponentSystemCache onDestroyComponentSystemCache ) 
            {
                List<OnDestroyComponentSystemCache> priorityList = onDestroyComponentSystemCache.SystemLifetimePriority switch
                {
                    SystemPriority.Early => OnDestroyEarlierPriority,
                    SystemPriority.Default => OnDestroyDefaultPriority,
                    SystemPriority.Late => OnDestroyLatePriority,
                    _ => OnDestroyDefaultPriority
                };
                
                priorityList.Add(onDestroyComponentSystemCache);
            }

            if (systemCache is OnCreateComponentSystemCache onCreatecomponentSystemCache ) 
            {
                List<OnCreateComponentSystemCache> priorityList = onCreatecomponentSystemCache.SystemLifetimePriority switch
                {
                    SystemPriority.Early => OnCreateEarlierPriority,
                    SystemPriority.Default => OnCreateDefaultPriority,
                    SystemPriority.Late => OnCreateLatePriority,
                    _ => OnCreateDefaultPriority
                };
                
                priorityList.Add(onCreatecomponentSystemCache);
            }
            
            if (systemCache is UpdateComponentSystemCache updateComponentSystemCache ) 
            {
                List<UpdateComponentSystemCache> priorityList = updateComponentSystemCache.SystemUpdatePriority switch
                {
                    SystemPriority.Early => UpdateEarlierPriority,
                    SystemPriority.Default => UpdateDefaultPriority,
                    SystemPriority.Late => UpdateLatePriority,
                    _ => UpdateDefaultPriority
                };
                
                priorityList.Add(updateComponentSystemCache);
            }
        }
        
        private void Remove(BaseSystemCache systemCache)
        {
            
            if(systemCache== null) return;

            if (systemCache is OnDestroyComponentSystemCache onDestroyComponentSystemCache ) 
            {
                List<OnDestroyComponentSystemCache> priorityList = onDestroyComponentSystemCache.SystemLifetimePriority switch
                {
                    SystemPriority.Early => OnDestroyEarlierPriority,
                    SystemPriority.Default => OnDestroyDefaultPriority,
                    SystemPriority.Late => OnDestroyLatePriority,
                    _ => OnDestroyDefaultPriority
                };
                
                priorityList.Remove(onDestroyComponentSystemCache);
            }

            if (systemCache is OnCreateComponentSystemCache onCreatecomponentSystemCache ) 
            {
                List<OnCreateComponentSystemCache> priorityList = onCreatecomponentSystemCache.SystemLifetimePriority switch
                {
                    SystemPriority.Early => OnCreateEarlierPriority,
                    SystemPriority.Default => OnCreateDefaultPriority,
                    SystemPriority.Late => OnCreateLatePriority,
                    _ => OnCreateDefaultPriority
                };
                
                priorityList.Remove(onCreatecomponentSystemCache);
            }
            
            if (systemCache is UpdateComponentSystemCache updateComponentSystemCache ) 
            {
                List<UpdateComponentSystemCache> priorityList = updateComponentSystemCache.SystemUpdatePriority switch
                {
                    SystemPriority.Early => UpdateEarlierPriority,
                    SystemPriority.Default => UpdateDefaultPriority,
                    SystemPriority.Late => UpdateLatePriority,
                    _ => UpdateDefaultPriority
                };
                
                priorityList.Remove(updateComponentSystemCache);
            }
        }


    }
    
    
    
    
}
