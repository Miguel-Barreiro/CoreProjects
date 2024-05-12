using System;
using System.Collections.Generic;
using Core.Utils.Reflection;

namespace Core.Systems
{
    public sealed class SystemsContainer
    {
        private readonly Dictionary<Type, HashSet<Object>> systemsByInterface = new Dictionary<Type, HashSet<Object>>();
        private readonly List<Object> systems = new List<Object>();
        #region Public

        public Action<Object> OnSystemAdded;
        public Action<Object> OnSystemRemoved;
        
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
            OnSystemAdded?.Invoke(system);
        }
        
        public void RemoveSystem(Object system)
        {
            Type objectType = system.GetType();
            RemoveFromInterfaces(system, objectType);
            systems.Remove(system);
            OnSystemRemoved?.Invoke(system);
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