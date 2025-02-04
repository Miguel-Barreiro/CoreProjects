﻿using System;
using System.Collections.Generic;
using Core.Systems;

#nullable enable

namespace Core.Model
{
    
    public class EntitiesContainer : IInitSystem
    {

        // ENTITY LIFETIME MANAGEMENT
        private readonly Dictionary<EntId, BaseEntity> entitiesByID = new ();

        // COMPONENT CACHING
        private readonly Dictionary<Type, List<BaseEntity>> entitiesByComponentType = new ();
        
        //EVENT PROCESSING
        private readonly List<BaseEntity> destroyedEntities = new ();
        private readonly List<BaseEntity> newEntities = new ();
        
        
        private int nextEntityID = 0;

        private static EntitiesContainer? instance = null!;
        
        public static EntitiesContainer CreateInstance()
        {
            if (instance == null)
            {
                instance = new EntitiesContainer();
            }
            
            return instance;
        }

        public static void Reset()
        {
            if (instance == null)
            {
                instance = new();
                return;
            }
            
            instance.entitiesByID.Clear();

            instance.entitiesByComponentType.Clear();
        
            instance.destroyedEntities.Clear();
            instance.newEntities.Clear();
            
        }

        private EntitiesContainer(){ }
        
        public T? GetEntity<T>(EntId id) where T:BaseEntity
        {
            return entitiesByID.GetValueOrDefault(id) as T;
        }

        public BaseEntity? GetEntity(EntId id)
        {
            return entitiesByID.GetValueOrDefault(id);
        }

        
        public int NewEntitiesCount() => newEntities.Count;
        
        public IEnumerable<BaseEntity> GetAllNewEntities()
        {
            foreach (BaseEntity newEntity in newEntities)
            {
                yield return newEntity;
            }
        }

        public int DestroyedEntitiesCount => destroyedEntities.Count;
        public IEnumerable<BaseEntity> GetAllDestroyedEntities()
        {
            foreach (BaseEntity destroyedEntity in destroyedEntities)
            {
                yield return destroyedEntity;
            }
        }
        
        public void UpgradeCurrentNewEntities()
        {
            foreach (BaseEntity newEntity in newEntities)
            {
                AddEntityInternal(newEntity);
            }
            newEntities.Clear();
        }
        
        public void ClearDestroyedEntities()
        {
            foreach (BaseEntity destroyedEntity in destroyedEntities)
            {
                RemoveEntityInternal(destroyedEntity);
            }
            destroyedEntities.Clear();
        }

        
        #region Initialization

        public void Initialize()
        {
            entitiesByComponentType.Clear();
            IEnumerable<Type> entityTypes = TypeCache.Get().GetAllEntityTypes();
            foreach (Type entityType in entityTypes)
            {
                if(!entitiesByComponentType.ContainsKey(entityType))
                    entitiesByComponentType.Add(entityType, new List<BaseEntity>());
            }
        }

        #endregion
        

        #region Entity Lifetime Management

        internal static EntId GenerateNewEntityID()
        {
            return new EntId(instance!.nextEntityID++);
        }
        
        internal static void OnEntityCreated(BaseEntity entity)
        {
            instance!.newEntities.Add(entity);
        }
        
        internal static void OnDestroyEntity(BaseEntity entity)
        {
            instance!.destroyedEntities.Add(entity);
        }
        
        public IEnumerable<TEntity> GetAllEntitiesByType<TEntity>() where TEntity : class, IEntity
        {
            if (!entitiesByComponentType.TryGetValue(typeof(TEntity), out List<BaseEntity> entities))
            {
                // Debug.Log($"no entities of type {typeof(TEntity)} found");
                yield break;
            }

            foreach (BaseEntity entity in entities)
            {
                yield return (entity as TEntity)!;
            }
        }

        
        private void AddEntityInternal(BaseEntity entity)
        {
            Type entityType = entity.GetType();

            entitiesByID.Add(entity.ID, entity);

            IEnumerable<Type> components = TypeCache.Get().GetComponentsOf(entityType);
            foreach (Type componentType in components)
            {
                if (entitiesByComponentType.TryGetValue(componentType, out List<BaseEntity> entities))
                {
                    entities.Add(entity);
                }
            }
        }
        
        private void RemoveEntityInternal(BaseEntity entity)
        {
            Type entityType = entity.GetType();
            
            IEnumerable<Type> components = TypeCache.Get().GetComponentsOf(entityType);
            foreach (Type componentType in components)
            {
                if (entitiesByComponentType.TryGetValue(componentType, out List<BaseEntity> entities))
                {
                    entities.Remove(entity);
                }
            }

            entitiesByID.Remove(entity.ID);
        }

        #endregion



    }

}