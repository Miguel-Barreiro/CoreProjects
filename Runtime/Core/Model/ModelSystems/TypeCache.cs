using System;
using System.Collections.Generic;
using Core.Utils.Reflection;
using UnityEngine;

namespace Core.Model
{
    public sealed class TypeCache
    {
        private readonly Dictionary<Type, List<Type>> componentsByEntityType = new ();
        private readonly List<Type> entityTypes = new ();
        
        internal TypeCache()
        {
            componentsByEntityType.Clear();
            entityTypes.Clear();

            IEnumerable<Type> types = ReflectionUtils.GetAllTypesOf<BaseEntity>();
            foreach (Type entityType in types)
            {
                BuildEntityTypeCache(entityType);
            }
        }
        
        public IEnumerable<Type> GetAllEntityTypes()
        {
            foreach (Type entityType in entityTypes)
            {
                yield return entityType;
            }
        }

        
        public IEnumerable<Type> GetComponentsOf(Type entityType)
        {
            if (!componentsByEntityType.TryGetValue(entityType, out List<Type> componentTypes))
            {
                Debug.LogError($"entityType {entityType} not found in cache");
                yield break;
            }
            
            foreach (Type componentType in componentTypes)
            {
                yield return componentType;
            }
        }
        
        #region Internal
        
        
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
            }
        }
        
        
        #endregion

    }
}