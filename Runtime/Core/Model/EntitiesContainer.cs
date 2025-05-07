using System;
using System.Collections.Generic;
using Core.Systems;

#nullable enable

namespace Core.Model
{


    public interface IEntitiesContainer
    {
        public T? GetEntity<T>(EntId id) where T : Entity;
        public Entity? GetEntity(EntId id);
        public Entity? GetNewEntity(EntId id);
        public IEnumerable<TEntity> GetAllEntitiesByType<TEntity>() where TEntity : class, IEntity;
        public bool IsEntityOfType<TEntity>(EntId id) where TEntity : class, IEntity;
        public bool IsEntityOfType<TEntity>(EntId id, out TEntity? castEntity) where TEntity : class, IEntity;

    }

    public class EntitiesContainer : IInitSystem, IEntitiesContainer
    {

        // ENTITY LIFETIME MANAGEMENT
        private readonly Dictionary<EntId, Entity> entitiesByID = new ();

        // COMPONENT CACHING
        private readonly Dictionary<Type, List<Entity>> entitiesByComponentType = new ();
        
        //EVENT PROCESSING
        private readonly List<Entity> destroyedEntities = new ();
        private readonly List<Entity> newEntities = new ();
        
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
        
        public T? GetEntity<T>(EntId id) where T:Entity
        {
            return entitiesByID.GetValueOrDefault(id) as T;
        }


        
        public Entity? GetEntity(EntId id)
        {
            return entitiesByID.GetValueOrDefault(id);
        }
        
        public Entity? GetNewEntity(EntId id)
        {
            return newEntities.Find(newEntity => newEntity.ID == id);
        }

        
        public bool IsEntityOfType<TEntity>(EntId id) where TEntity : class, IEntity
        {
            Entity? baseEntity = GetEntity(id);
            if (baseEntity == null)
                return false;

            return baseEntity is TEntity;
        }

        public bool IsEntityOfType<TEntity>(EntId id, out TEntity? castEntity) where TEntity : class, IEntity
        {
            Entity? baseEntity = GetEntity(id);
            if (baseEntity == null)
            {
                castEntity = null;
                return false;
            }
            castEntity = baseEntity as TEntity;
            return castEntity != null;
        }


        public int NewEntitiesCount() => newEntities.Count;
        
        public IEnumerable<Entity> GetAllNewEntities()
        {
            foreach (Entity newEntity in newEntities)
            {
                yield return newEntity;
            }
        }

        public int destroyedEntitiesCount => destroyedEntities.Count;
        public IEnumerable<Entity> GetAllDestroyedEntities()
        {
            foreach (Entity destroyedEntity in destroyedEntities)
            {
                yield return destroyedEntity;
            }
        }

        
        private readonly List<Entity> FlushedDeadEntities = new();
        private readonly List<Entity> FlushedNewEntities = new();
        
        
        public void FlushCurrentDestroyedEntities()
        {
            FlushedDeadEntities.AddRange(destroyedEntities);
            destroyedEntities.Clear();
        }
        
        public void FlushCurrentNewEntities()
        {
            FlushedNewEntities.AddRange(newEntities);
            newEntities.Clear();
        }
        
        
        public void ClearAllFlushedDeadEntities()
        {
            foreach (Entity destroyedEntity in FlushedDeadEntities)
                RemoveEntityInternal(destroyedEntity);
            
            FlushedDeadEntities.Clear();
        }
        
        public void UpgradeAllFlushedNewEntities()
        {
            foreach (Entity newEntity in FlushedNewEntities)
                AddEntityInternal(newEntity);

            FlushedNewEntities.Clear();
        }

        
        #region Initialization

        public void Initialize()
        {
            entitiesByComponentType.Clear();
            IEnumerable<Type> entityTypes = TypeCache.Get().GetAllEntityTypes();
            foreach (Type entityType in entityTypes)
            {
                if(!entitiesByComponentType.ContainsKey(entityType))
                    entitiesByComponentType.Add(entityType, new List<Entity>());
            }
        }

        #endregion
        

        #region Entity Lifetime Management

        internal static EntId GenerateNewEntityID()
        {
            return new EntId(instance!.nextEntityID++);
        }
        
        internal static void OnEntityCreated(Entity entity)
        {
            instance!.newEntities.Add(entity);
        }
        
        internal static void OnDestroyEntity(Entity entity)
        {
            if(instance!.destroyedEntities.Contains(entity))
                return;
            
            instance!.destroyedEntities.Add(entity);
        }
        
        public IEnumerable<TEntity> GetAllEntitiesByType<TEntity>() where TEntity : class, IEntity
        {
            if (!entitiesByComponentType.TryGetValue(typeof(TEntity), out List<Entity> entities))
            {
                // Debug.Log($"no entities of type {typeof(TEntity)} found");
                yield break;
            }

            foreach (Entity entity in entities)
            {
                yield return entity! as TEntity;
            }
        }

        
        private void AddEntityInternal(Entity entity)
        {
            Type entityType = entity.GetType();

            entitiesByID.Add(entity.ID, entity);

            IEnumerable<Type> components = TypeCache.Get().GetComponentsOf(entityType);
            foreach (Type componentType in components)
            {
                if (entitiesByComponentType.TryGetValue(componentType, out List<Entity> entities))
                {
                    entities.Add(entity);
                }
            }
        }
        
        private void RemoveEntityInternal(Entity entity)
        {
            Type entityType = entity.GetType();
            
            IEnumerable<Type> components = TypeCache.Get().GetComponentsOf(entityType);
            foreach (Type componentType in components)
            {
                if (entitiesByComponentType.TryGetValue(componentType, out List<Entity> entities))
                {
                    entities.Remove(entity);
                }
            }

            entitiesByID.Remove(entity.ID);
        }

        #endregion



    }

}