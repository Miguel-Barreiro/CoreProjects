using System;
using System.Collections.Generic;
using System.ComponentModel;
using Core.Events;
using Core.Model.ModelSystems;
using Core.Utils.CachedDataStructures;
using Core.Utils.Reflection;
using UnityEngine;
using Component = UnityEngine.Component;

#nullable enable

namespace Core.Model
{
    
    public interface ITypeCache
    {
        IEnumerable<Type> GetAllEntityTypes();
        IEnumerable<Type> GetAllEntityComponentTypes();
        IEnumerable<Type> GetAllComponentDataTypes();
        IEnumerable<Type> GetAllEventTypes();
        IEnumerable<TypeCache.EventAttributes> GetAllEventAttributes();
        TypeCache.EventAttributes? GetEventAttributes(Type eventType);
        IEnumerable<Type> GetAllEntityEventTypes();
        IEnumerable<Type> GetComponentsOf(Type entityType);
    }
    
    public sealed class TypeCache: ITypeCache
    {
        private readonly Dictionary<Type, List<Type>> componentsBySystemType = new ();
        private readonly Dictionary<Type, List<Type>> componentsByEntityType = new ();
        private readonly Dictionary<Type, List<Type>> componentDatasByEntityType = new ();
        private readonly List<Type> entityTypes = new ();
        private readonly List<Type> componentTypes = new ();
        private readonly List<Type> componentDataTypes = new ();
        private readonly List<Type> eventTypes = new();
        private readonly List<Type> entityEventTypes = new();
        
        private readonly Dictionary<Type, Type> componentDataTypeByComponentType = new();
        
        private readonly Dictionary<Type,EventAttributes> EventAttributesByType = new();

        private static TypeCache? instance = null;
        public static TypeCache Get()
        {
            if (instance == null)
            {
                instance = new TypeCache();
            }
            return instance;
        }
        
        private TypeCache()
        {
            componentsByEntityType.Clear();
            entityTypes.Clear();

            BuildComponentsTypeCache();
            BuildComponentDataTypeCache();
            
            IEnumerable<Type> types = ReflectionUtils.GetAllTypesOf<Entity>();
            foreach (Type entityType in types)
            {
                BuildEntityTypeCache(entityType);
            }

            BuildEventTypeCache();
        }

        private void BuildComponentsTypeCache()
        {
            IEnumerable<Type> components = ReflectionUtils.GetAllTypesImplementingGenericDefinition(typeof(Component<>));
            foreach (Type currentPotentialComponentType in components)
            {
                if(currentPotentialComponentType.IsInterface)
                    componentTypes.Add(currentPotentialComponentType);
            }
        }


        private void BuildComponentDataTypeCache()
        {

            IEnumerable<Type> types = ReflectionUtils.GetAllTypesOf<IComponentData>();
            foreach (Type currentPotentialComponentDataType in types)
            {
                if (currentPotentialComponentDataType.IsValueType)
                {
                    componentDataTypes.Add(currentPotentialComponentDataType);
                }
            }

            foreach (Type componentType in componentTypes)
            {
                Type componentDataType = componentType.GetFirstGenericArgumentTypeDefinition(typeof(Component<>));
                if (componentDataType == null)
                {
#if UNITY_EDITOR
                    Debug.LogError($"could not found componentData for {componentType}");
#endif                    
                    continue;
                }
                
                if (!componentDataTypes.Contains(componentDataType))
                {
#if UNITY_EDITOR
                    Debug.LogError($"Found invalid componentData {componentDataType} for component {componentType}");
#endif                    
                   continue;
                }
                if (!componentDataTypeByComponentType.ContainsKey(componentType))
                    componentDataTypeByComponentType.Add(componentType, componentDataType);
            }
        }


        public IEnumerable<EventAttributes> GetAllEventListenerTypes()
        {
            foreach ((Type _, EventAttributes eventAttributes) in EventAttributesByType)
            {
                yield return eventAttributes;
            }
        }

        public EventAttributes? GetEventAttributes(Type eventType)
        {
            return EventAttributesByType.GetValueOrDefault(eventType);
        }

        public IEnumerable<EventAttributes> GetAllEventAttributes()
        {
            foreach ((Type _, EventAttributes eventAttributes) in EventAttributesByType)
            {
                yield return eventAttributes;
            }
        }

        
        
        public IEnumerable<Type> GetAllEventTypes()
        {
            foreach (Type eventType in eventTypes)
            {
                yield return eventType;
            }
        }
        
        public IEnumerable<Type> GetAllEntityEventTypes()
        {
            foreach (Type eventType in entityEventTypes)
            {
                yield return eventType;
            }
        }

        public IEnumerable<Type> GetAllEntityTypes()
        {
            foreach (Type entityType in entityTypes)
            {
                yield return entityType;
            }

            yield return typeof(Entity);
        }

        public Type GetComponentDataTypeFromComponentType(Type componentType)
        {
            return componentDataTypeByComponentType[componentType];
        }

        public IEnumerable<Type> GetComponentsOf(Type entityType)
        {
            if (!componentsByEntityType.TryGetValue(entityType, out List<Type> componentTypes))
            {
                // Debug.LogError($"entityType {entityType} not found in cache");
                yield break;
            }
            
            foreach (Type componentType in componentTypes)
            {
                yield return componentType;
            }
        }

        public IEnumerable<Type> GetComponentDatasOfEntityType(Type entityType)
        {
            if (!componentsByEntityType.TryGetValue(entityType, out List<Type> componentTypes))
            {
                // Debug.LogError($"entityType {entityType} not found in cache");
                yield break;
            }
            
            foreach (Type componentType in componentTypes)
            {
                yield return componentDataTypeByComponentType[componentType];
            }
        }

        
        
        public IEnumerable<Type> GetAllEntityComponentTypes()
        {
            foreach (Type componentType in componentTypes)
            {
                yield return componentType;
            }
        }

        public IEnumerable<Type> GetAllComponentDataTypes()
        {
            foreach (Type componentType in componentDataTypes)
            {
                yield return componentType;
            }
        }

        #region Internal
        

        
        private void BuildEventTypeCache()
        {
            IEnumerable<Type> allEventTypes = ReflectionUtils.GetAllTypesOf<BaseEvent>();
            foreach (Type potentialEventType in allEventTypes)
            {
                if (potentialEventType.IsGenericType)
                {
                    continue;
                }
                eventTypes.Add(potentialEventType);
            }
            
            foreach (Type eventType in eventTypes)
            {
                EventAttributesByType.Add(eventType, new EventAttributes(eventType));
            }
            
            
            IEnumerable<Type> allEntityEventTypes = ReflectionUtils.GetAllTypesOf<BaseEntityEvent>();
            foreach (Type potentialEntityEventType in allEntityEventTypes)
            {
                if (potentialEntityEventType.IsGenericType)
                {
                    continue;
                }
                entityEventTypes.Add(potentialEntityEventType);
            }
        }

        
        private void BuildEntityTypeCache(Type entityType)
        {
            
            if(!componentsByEntityType.ContainsKey(entityType)){
                
                List<Type> cachedComponentTypeList = new List<Type>();
                componentsByEntityType.Add(entityType, cachedComponentTypeList);
                
                IEnumerable<Type> entityComponentTypes = GetEntityComponentTypes(entityType);
                cachedComponentTypeList.AddRange(entityComponentTypes);
            }
            
            if(!componentDatasByEntityType.ContainsKey(entityType))
            {
                List<Type> cachedComponentDataTypeList = new List<Type>();
                componentDatasByEntityType.Add(entityType, cachedComponentDataTypeList);
                
                IEnumerable<Type> entityComponentTypes = GetEntityComponentTypes(entityType);
                foreach (Type componentType in entityComponentTypes)
                {
                    if (!componentDataTypeByComponentType.TryGetValue(componentType, out Type componentDataType))
                    {
#if UNITY_EDITOR
                        Debug.LogError($"could not found componentData for {componentType}");
#endif
                        continue;
                    }


                    if (componentDataType != null && !cachedComponentDataTypeList.Contains(componentDataType))
                        cachedComponentDataTypeList.Add(componentDataType);
                }
            }
            
            if (!entityTypes.Contains(entityType))
            {
                entityTypes.Add(entityType);
            }

            static IEnumerable<Type> GetEntityComponentTypes(Type entityType)
            {
                using CachedList<Type> result = ListCache<Type>.Get();
                
                IEnumerable<Type> implementedInterfaces = entityType.GetImplementedInterfaces();
                foreach (Type implementedInterface in implementedInterfaces)
                {

                    bool isActualComponentType = implementedInterface.IsGenericType && implementedInterface.GetGenericTypeDefinition() == typeof(Component<>);
                    if ( !isActualComponentType && implementedInterface.IsAssignableToGenericWithArgType(typeof(Component<>)) &&
                        !result.Contains(implementedInterface))
                        result.Add(implementedInterface);
                }

                foreach (Type implementedComponent in result)
                {
                    yield return implementedComponent;
                }
            }
        }
        
        
        public sealed class EventAttributes
        {
            public readonly Type EventType;
            public readonly EventOrder EventOrder;
            public readonly Type EventListenerType;
            public readonly Type PostEventListenerType;
            
            public EventAttributes(Type eventType)
            {
                EventType = eventType;
                
                Type genericEventListenerType = typeof(IEventListener<>);
                EventListenerType = genericEventListenerType.MakeGenericType(new []{eventType});
                
                Type genericPostEventListenerType = typeof(IPostEventListener<>);
                PostEventListenerType = genericPostEventListenerType.MakeGenericType(new []{eventType});
                
                
                if (eventType.IsTypeOf<IEarlyEvent>())
                {
                    this.EventOrder = EventOrder.PreDefault;
                    return;
                }
                if (eventType.IsTypeOf<ILateEvent>())
                {
                    this.EventOrder = EventOrder.PostDefault;
                    return;
                }

                this.EventOrder = EventOrder.Default;
            }
        }
        
        #endregion

        
    }
}