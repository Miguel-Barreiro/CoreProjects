using System;
using System.Collections.Generic;
using Core.Events;
using Core.Utils.Reflection;
using UnityEngine;

#nullable enable

namespace Core.Model
{
    public sealed class TypeCache
    {
        private readonly Dictionary<Type, List<Type>> componentsByEntityType = new ();
        private readonly List<Type> entityTypes = new ();
        private readonly List<Type> eventTypes = new();
        // private readonly List<(Type, Type)> eventListenerTypes = new();

        private readonly Dictionary<Type,EventAttributes> EventAttributesByType = new();

        private static TypeCache instance = null;
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

            IEnumerable<Type> types = ReflectionUtils.GetAllTypesOf<BaseEntity>();
            foreach (Type entityType in types)
            {
                BuildEntityTypeCache(entityType);
            }

            BuildEventTypeCache();
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

        public IEnumerable<Type> GetAllEntityTypes()
        {
            foreach (Type entityType in entityTypes)
            {
                yield return entityType;
            }

            yield return typeof(BaseEntity);
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
            
            Type genericEventListenerType = typeof(IEventListener<>);
            Type[] typeArgument = new []{genericEventListenerType};
            foreach (Type eventType in eventTypes)
            {
                // typeArgument[0] = eventType;
                // Type eventListenerType = genericEventListenerType.MakeGenericType(typeArgument);
                // eventListenerTypes.Add((eventType, eventListenerType));
                EventAttributesByType.Add(eventType, new EventAttributes(eventType));
            }
        }

        
        private void BuildEntityTypeCache(Type entityType)
        {
            if(!componentsByEntityType.ContainsKey(entityType)){
                
                List<Type> cachedComponentTypeList = new List<Type>();
                componentsByEntityType.Add(entityType, cachedComponentTypeList);
                
                IEnumerable<Type> componentTypes = GetEntityComponentTypes(entityType);
                cachedComponentTypeList.AddRange(componentTypes);
                cachedComponentTypeList.Add(entityType);

                foreach (Type componentType in cachedComponentTypeList)
                {
                    if (!entityTypes.Contains(componentType))
                    {
                        entityTypes.Add(componentType);
                    }
                }
            }
            
            static IEnumerable<Type> GetEntityComponentTypes(Type entityType)
            {
                IEnumerable<Type> implementedInterfaces = entityType.GetImplementedInterfaces();
                foreach (Type implementedInterface in implementedInterfaces)
                {
                    if (implementedInterface.IsTypeOf<IComponent>())
                    {
                        yield return implementedInterface;
                    }
                }
                yield return typeof(BaseEntity);
            }
        }
        
        
        public sealed class EventAttributes
        {
            public readonly Type EventType;
            public readonly EventOrder EventOrder;
            public readonly Type EventListenerType;
            
            public EventAttributes(Type eventType)
            {
                EventType = eventType;
                
                Type genericEventListenerType = typeof(IEventListener<>);
                EventListenerType = genericEventListenerType.MakeGenericType(new []{eventType});
                
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