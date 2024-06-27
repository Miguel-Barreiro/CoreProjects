using System;
using System.Collections.Generic;
using Core.Systems;
using Core.Utils.Reflection;
using UnityEngine;
using Zenject;

namespace Core.Model
{
    
    public class EntityLifetimeManager : IInitSystem
    {
        private readonly Dictionary<Type, EntitySystemsContainer.SystemCache> systemByType = new();
        private readonly Dictionary<Type, List<EntitySystemsContainer.SystemCache>> systemsByComponentType = new ();

        // COMPONENT CACHING
        private readonly Dictionary<Type, List<Type>> componentsByEntityType = new ();
        private readonly Dictionary<Type, List<BaseEntity>> entitiesByComponentType = new ();
        private readonly Dictionary<EntId, BaseEntity> entitiesByID = new ();
        
        private int nextEntityID = 0;

        private static EntityLifetimeManager instance;

        [Inject] private readonly SystemsContainer SystemsContainer = null!;
        
        internal static EntityLifetimeManager CreateInstance()
        {
            if (instance == null)
            {
                instance = new EntityLifetimeManager();
            }
            
            return instance;
        }
        private EntityLifetimeManager(){ }
        

        #region Initialization

        public void Initialize()
        {
            componentsByEntityType.Clear();
            entitiesByComponentType.Clear();
            
            systemsByComponentType.Clear();
            systemByType.Clear();
        }

        #endregion
        

        #region Entity Lifetime Management

        internal static EntId GenerateNewEntityID()
        {
            return new EntId(instance.nextEntityID++);
        }
        
        internal static void OnEntityCreated(BaseEntity entity)
        {
            instance.OnEntityCreatedInternal(entity);
        }
        
        internal static void OnDestroyEntity(BaseEntity entity)
        {
            instance.OnDestroyEntityInternal(entity);
        }

        
        private static readonly object[] ARGUMENT = { null };
        private void OnEntityCreatedInternal(BaseEntity entity)
        {
            Type entityType = entity.GetType();

            entitiesByID.Add(entity.ID, entity);
            
            BuildEntityTypeCacheIfNeeded(entityType);

            List<Type> components = componentsByEntityType[entityType];
            foreach (Type componentType in components)
            {
                List<BaseEntity> entities = entitiesByComponentType[componentType];
                entities.Add(entity);

                IEnumerable<EntitySystemsContainer.SystemCache> componentSystems = SystemsContainer.GetComponentSystemsFor(componentType);
                foreach (EntitySystemsContainer.SystemCache systemCache in componentSystems)
                {
                    BaseEntitySystem system = systemCache.System;
                    ARGUMENT[0] = entity;
                    systemCache.CachedOnNewEntityMethod?.Invoke(system, ARGUMENT);
                }
            }
        }
        
        private void OnDestroyEntityInternal(BaseEntity entity)
        {
            Type entityType = entity.GetType();

            entitiesByID.Add(entity.ID, entity);
            
            BuildEntityTypeCacheIfNeeded(entityType);

            List<Type> components = componentsByEntityType[entityType];
            foreach (Type componentType in components)
            {
                IEnumerable<EntitySystemsContainer.SystemCache> componentSystems = SystemsContainer.GetComponentSystemsFor(componentType);
                foreach (EntitySystemsContainer.SystemCache systemCache in componentSystems)
                {
                    BaseEntitySystem system = systemCache.System;
                    ARGUMENT[0] = entity;
                    systemCache.CachedOnEntityDestroyedMethod?.Invoke(system, ARGUMENT);
                }
                entitiesByComponentType[componentType].Remove(entity);
            }

            entitiesByID.Remove(entity.ID);
        }

        #endregion


        #region Internal
        
        
        private void BuildEntityTypeCacheIfNeeded(Type entityType)
        {
            if(!componentsByEntityType.ContainsKey(entityType)){
                List<Type> cachedComponentTypeList = new List<Type>();
                componentsByEntityType.Add(entityType, cachedComponentTypeList);
                
                IEnumerable<Type> componentTypes = GetEntityComponentTypes(entityType);
                cachedComponentTypeList.AddRange(componentTypes);
                cachedComponentTypeList.Add(entityType);
                
                foreach (Type componentType in cachedComponentTypeList)
                {
                    if (!entitiesByComponentType.ContainsKey(componentType))
                    {
                        entitiesByComponentType.Add(componentType, new List<BaseEntity>());
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
        
        public IEnumerable<TEntity> GetAllEntitiesByType<TEntity>() where TEntity : class, IEntity
        {
            if (!entitiesByComponentType.TryGetValue(typeof(TEntity), out List<BaseEntity> entities))
            {
                Debug.Log($"no entities of type {typeof(TEntity)} found");
                yield break;
            }

            foreach (BaseEntity entity in entities)
            {
                yield return entity as TEntity;
            }
        }

        public IEnumerable<IComponent> GetAllEntitiesByType(Type componentType)
        {
            entitiesByComponentType.TryGetValue(componentType, out List<BaseEntity> entities);
            
            foreach (BaseEntity entity in entities)
            {
                yield return entity as IComponent;
            }
        }

        
        #endregion


    }

}