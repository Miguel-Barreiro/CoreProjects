using System;
using System.Collections.Generic;
using Core.Model;
using Core.Utils.Reflection;

namespace Core.Systems
{
    public sealed class SystemsContainer : IInitSystem
    {
        private readonly Dictionary<Type, HashSet<Object>> systemsByInterface = new Dictionary<Type, HashSet<Object>>();
        private readonly List<Object> systems = new List<Object>();

        private readonly ComponentSystemsContainer componentSystemsContainer = new ComponentSystemsContainer();
        
        #region Public
        
        public IEnumerable<T> GetAllSystemsByInterface<T>() where T : class
        {
            Type interfaceType = typeof(T);
            if (systemsByInterface.TryGetValue(interfaceType, out var byInterface))
            {
                foreach (Object implements in byInterface)
                {
                    yield return implements as T;
                }
            }
        }
        
        public void AddSystem(Object system)
        {
            Type objectType = system.GetType();
            AddToInterfaces(system, objectType);
            systems.Add(system);
            if (objectType.IsTypeOf<BaseComponentSystem>())
            {
                componentSystemsContainer.AddComponentSystem(system as BaseComponentSystem);
            }
        }
        
        public void RemoveSystem(Object system)
        {
            Type objectType = system.GetType();
            RemoveFromInterfaces(system, objectType);
            systems.Remove(system);
            if (objectType.IsTypeOf<BaseComponentSystem>())
            {
                componentSystemsContainer.RemoveComponentSystem(system as BaseComponentSystem);
            }
        }


        #endregion

        #region Internal
        
        
        public void Initialize()
        {
            componentSystemsContainer.Init();
            
        }

        
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

        public IEnumerable<ComponentSystemsContainer.SystemCache> GetComponentSystemsFor(Type componentType)
        {
            return componentSystemsContainer.GetComponentSystemsFor(componentType);
        }

        public IEnumerable<BaseComponentSystem> GetAllComponentSystems()
        {
            return componentSystemsContainer.GetAllComponentSystems();
        }
    }
}