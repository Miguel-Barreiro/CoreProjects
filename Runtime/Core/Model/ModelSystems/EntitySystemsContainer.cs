using System;
using System.Collections.Generic;
using System.Linq;
using Core.Model.ModelSystems.ComponentSystems;
using Core.Systems;
using Core.Utils.Reflection;
using UnityEngine;

namespace Core.Model.ModelSystems
{


    public class EntitySystemsContainer
    {

        private readonly List<Type> EarlyComponentData= new List<Type>();
        private readonly List<Type> DefaultComponentData= new List<Type>();
        private readonly List<Type> LateComponentData= new List<Type>();
        
        private readonly Dictionary<Type, ComponentSystemListenerGroup> SystemsCacheByComponentDataType = new ();
        private readonly List<object> systems = new();


        internal EntitySystemsContainer()
        {
            foreach (Type componentDataType in TypeCache.Get().GetAllComponentDataTypes())
            {
                SystemsCacheByComponentDataType.Add(componentDataType, new ComponentSystemListenerGroup());
    
                ComponentDataAttribute system = 
                    ReflectionUtils.GetAttributesOfType<ComponentDataAttribute>(componentDataType);
                SystemPriority systemPriority = system?.Priority ?? SystemPriority.Default;

                List<Type> target = systemPriority switch
                {
                    SystemPriority.Early => EarlyComponentData,
                    SystemPriority.Default => DefaultComponentData,
                    SystemPriority.Late => LateComponentData,
                    _ => DefaultComponentData
                };
                target.Add(componentDataType);
            }
        }
        
        
        
        #region Public
        
        

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
            if (!systems.Contains(system)) return;

            foreach (ComponentSystemListenerGroup listenerGroup in SystemsCacheByComponentDataType.Values)
                listenerGroup.RemoveSystem(system);

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
