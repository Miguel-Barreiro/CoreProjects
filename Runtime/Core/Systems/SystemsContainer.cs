using System;
using System.Collections.Generic;
using Core.Events;
using Core.Model;
using Core.Utils.Reflection;

namespace Core.Systems
{
    public sealed class SystemsContainer
    {
        private readonly Dictionary<Type, HashSet<Object>> systemsByInterface = new Dictionary<Type, HashSet<Object>>();
        private readonly List<Object> systems = new List<Object>();

        private readonly EntitySystemsContainer entitySystemsContainer = new EntitySystemsContainer();
        private readonly EventListenerSystemsContainer eventListenerSystemsContainer = new EventListenerSystemsContainer();

        private readonly Dictionary<Object, string > SystemByInstallerName = new Dictionary<Object, string>();
        
        #region Public
        
        public IEnumerable<Object> GetAllSystems()
        {
            return systems;
        }
        
        public IEnumerable<EntitySystemsContainer.SystemCache> GetComponentSystemsFor(Type componentType, SystemPriority priority)
        {
            return entitySystemsContainer.GetComponentSystemsFor(componentType, priority);
        }
        

        public IEnumerable<EntitySystemsContainer.SystemCache> GetComponentSystemsForDestroyed(Type componentType, SystemPriority priority)
        {
            return entitySystemsContainer.GetComponentSystemsForDestroyed(componentType, priority);
        }


        public IEnumerable<(Type, EntitySystemsContainer.SystemListenerGroup)> GetAllEntitySystemsByComponentType()
        {
            return entitySystemsContainer.GetAllComponentSystemsByComponentType();
        }


        public IEnumerable<BaseEntitySystem> GetAllComponentSystems()
        {
            return entitySystemsContainer.GetAllComponentSystems();
        }
        
        public IEnumerable<T> GetAllSystemsByInterface<T>() where T : class
        {
            Type interfaceType = typeof(T);
            if (systemsByInterface.TryGetValue(interfaceType, out var byInterface))
            {
                foreach (Object implements in byInterface)
                {
                    yield return (T)implements;
                }
            }
        }
        
        public IEnumerable<Object> GetAllSystemsByInterface(Type interfaceType)
        {
            if (systemsByInterface.TryGetValue(interfaceType, out var byInterface))
            {
                foreach (Object implements in byInterface)
                {
                    yield return implements;
                }
            }
        }


        public IEnumerable<EventListenerSystemsContainer.EventListenerSystemCache> GetAllEventListenerSystems<TEvent>() 
            where TEvent : Event<TEvent>, new()
        {
            return eventListenerSystemsContainer.GetAllEventListenerSystems<TEvent>();
        }
        

        public IEnumerable<EventListenerSystemsContainer.EventListenerSystemCache> GetAllPostEventListenerSystems<TEvent>() 
            where TEvent : Event<TEvent>, new()
        {
            return eventListenerSystemsContainer.GetAllPostEventListenerSystems<TEvent>();
        }

        
        public void AddSystem(object system, string name)
        {
            Type objectType = system.GetType();
            AddToInterfaces(system, objectType);
            systems.Add(system);
            
            SystemByInstallerName[system] = name;
            
            if (objectType.IsTypeOf<BaseEntitySystem>())
            {
                entitySystemsContainer.AddComponentSystem(system as BaseEntitySystem);
            }

            if (objectType.IsAssignableToGenericType(typeof(IEventListener<>)))
            {
                eventListenerSystemsContainer.AddEventListener(system);
            }

            if (objectType.IsAssignableToGenericType(typeof(IPostEventListener<>)))
            {
                eventListenerSystemsContainer.AddPostEventListener(system);
            }

        }
        
        public void RemoveSystem(Object system)
        {
            Type objectType = system.GetType();
            RemoveFromInterfaces(system, objectType);
            systems.Remove(system);
            
            SystemByInstallerName.Remove(system);
            
            if (objectType.IsTypeOf<BaseEntitySystem>())
            {
                entitySystemsContainer.RemoveComponentSystem(system as BaseEntitySystem);
            }
            
            if (objectType.IsAssignableToGenericType(typeof(IEventListener<>)))
            {
                eventListenerSystemsContainer.RemoveEventListener(system);
            }
            
            if (objectType.IsAssignableToGenericType(typeof(IPostEventListener<>)))
            {
                eventListenerSystemsContainer.RemovePostEventListener(system);
            }

        }

        
        public string GetInstallerName(Object system)
        {
            if (SystemByInstallerName.TryGetValue(system, out string name))
            {
                return name;
            }
            return "NONE";
        }

        #endregion

        #region Internal
        
        
        private void AddToInterfaces<T>(T system, Type objectType)
        {
            IEnumerable<Type> implementedInterfaces = objectType.GetImplementedInterfaces();
            foreach (Type implementedInterface in implementedInterfaces)
            {
                if (!systemsByInterface.ContainsKey(implementedInterface))
                {
                    systemsByInterface[implementedInterface] = new HashSet<Object>();
                }
                systemsByInterface[implementedInterface].Add(system);
            }
        }

        private void RemoveFromInterfaces<T>(T system, Type objectType)
        {
            IEnumerable<Type> implementedInterfaces = objectType.GetImplementedInterfaces();
            foreach (Type implementedInterface in implementedInterfaces)
            {
                systemsByInterface[implementedInterface].Remove(system);
            }
        }

        #endregion


        
    }
}