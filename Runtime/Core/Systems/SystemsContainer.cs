using System;
using System.Collections.Generic;
using Core.Events;
using Core.Model.ModelSystems;
using Core.Utils.Reflection;
using Zenject;

namespace Core.Systems
{
    public sealed class SystemsContainer
    {
        
        private readonly Dictionary<Type, HashSet<Object>> systemsByInterface = new Dictionary<Type, HashSet<Object>>();
        private readonly List<Object> systems = new List<Object>();

        private readonly EntitySystemsContainer EntitySystemsContainer;
        private readonly EventListenerSystemsContainer eventListenerSystemsContainer = new EventListenerSystemsContainer();

        private readonly Dictionary<Object, string > SystemByInstallerName = new Dictionary<Object, string>();

        internal SystemsContainer(EntitySystemsContainer entitySystemsContainer)
        {
            EntitySystemsContainer = entitySystemsContainer;
        }

        #region Public
        
        public IEnumerable<Object> GetAllSystems()
        {
            return systems;
        }
        

        internal IEnumerable<KeyValuePair<Type, ComponentSystemListenerGroup>> GetAllEntitySystemsByComponentDataType()
        {
            return EntitySystemsContainer.GetAllComponentSystemsByComponentDataType();
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
            Type systemType = system.GetType();
            AddToInterfaces(system, systemType);
            systems.Add(system);
            
            SystemByInstallerName[system] = name;
            
            if (ComponentUtils.IsComponentSystem(systemType))
            {
                EntitySystemsContainer.AddSystem(system);
            }

            if (systemType.IsAssignableToGenericWithArgType(typeof(IEventListener<>)))
            {
                eventListenerSystemsContainer.AddEventListener(system);
            }

            if (systemType.IsAssignableToGenericWithArgType(typeof(IPostEventListener<>)))
            {
                eventListenerSystemsContainer.AddPostEventListener(system);
            }

        }
        
        public void RemoveSystem(Object system)
        {
            Type systemType = system.GetType();
            RemoveFromInterfaces(system, systemType);
            systems.Remove(system);
            
            SystemByInstallerName.Remove(system);
            
            EntitySystemsContainer.RemoveComponentSystem(system);
            
            if (systemType.IsAssignableToGenericWithArgType(typeof(IEventListener<>)))
            {
                eventListenerSystemsContainer.RemoveEventListener(system);
            }
            
            if (systemType.IsAssignableToGenericWithArgType(typeof(IPostEventListener<>)))
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