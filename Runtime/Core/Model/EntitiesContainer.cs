using System;
using System.Collections.Generic;
using Core.Model.ModelSystems;
using Core.Systems;

#nullable enable

namespace Core.Model
{


    public interface IEntitiesContainer
    {
        public T? GetEntity<T>(EntId id) where T : class, IEntity;
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
        private readonly Dictionary<Type, List<EntId>> entitiesByComponentType = new ();
        
        //EVENT PROCESSING
        private readonly List<EntId> destroyedEntities = new ();
        private readonly List<Entity> newEntities = new ();
        
        private uint nextEntityID = 0;

        private static EntitiesContainer? instance = null!;
        

        public static void DestroyEntity(EntId entityId)
        {
            if(!(instance!.destroyedEntities.Contains(entityId)))
                instance!.destroyedEntities.Add(entityId);
        }


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
        
        public T? GetEntity<T>(EntId id) where T : class, IEntity
        {
            return entitiesByID.GetValueOrDefault(id) as T;
        }


        
        public Entity? GetEntity(EntId id)
        {
            return entitiesByID.GetValueOrDefault(id);
        }
        
        public Entity? GetNewEntity(EntId entID)
        {
           return newEntities.Find(entity => entity.ID == entID);
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


        public int newEntitiesCount => newEntities.Count;
        
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
            foreach (EntId destroyedEntityId in destroyedEntities)
            {
                Entity? destroyedEntity = GetEntity(destroyedEntityId);
                if(destroyedEntity == null)
                    destroyedEntity = newEntities.Find(entity => entity.ID == destroyedEntityId);
    
                if(destroyedEntity != null)
                    yield return destroyedEntity;
            }
        }

        
        private readonly List<Entity> FlushedDeadEntities = new();
        private readonly List<Entity> FlushedNewEntities = new();
        
        
        internal void FlushCurrentDestroyedEntities()
        {
            FlushedDeadEntities.AddRange(GetAllDestroyedEntities());
            destroyedEntities.Clear();
        }
        internal void FlushCurrentNewEntities()
        {
            FlushedNewEntities.AddRange(newEntities);
            newEntities.Clear();
        }
        


        internal void ProcessAllFlushedEntities()
        {
            UpgradeAllFlushedNewEntities();
            ClearAllFlushedDeadEntities();
        }

        private void ClearAllFlushedDeadEntities()
        {
            foreach (Entity destroyedEntity in FlushedDeadEntities)
                RemoveEntityInternal(destroyedEntity);
            
            FlushedDeadEntities.Clear();
        }

        private void UpgradeAllFlushedNewEntities()
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
                    entitiesByComponentType.Add(entityType, new List<EntId>());

                IEnumerable<Type> components = TypeCache.Get().GetComponentsOf(entityType);
                foreach (Type componentType in components)
                {
                    if (!entitiesByComponentType.ContainsKey(componentType))
                        entitiesByComponentType.Add(componentType, new List<EntId>());
                }
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

            DataContainersControllerImplementation dataContainersController = DataContainersControllerImplementation.GetInstance();
            IEnumerable<Type> componentDatasType = TypeCache.Get().GetComponentDatasOfEntityType(entity.GetType());
            foreach (Type componentDataType in componentDatasType)
            {
                object componentContainer = dataContainersController.GetComponentContainer(componentDataType);
                ((IGenericComponentContainer)componentContainer).SetupComponent(entity.ID);
            }
        }
        
        
        public IEnumerable<TEntity> GetAllEntitiesByType<TEntity>() where TEntity : class, IEntity
        {
            if (!entitiesByComponentType.TryGetValue(typeof(TEntity), out List<EntId> entitieIds))
            {
                // Debug.Log($"no entities of type {typeof(TEntity)} found");
                yield break;
            }

            foreach (EntId entityId in entitieIds)
            {
                Entity entity = entitiesByID[entityId];
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
                if (entitiesByComponentType.TryGetValue(componentType, out List<EntId> entities))
                    entities.Add(entity.ID);
            }
        }
        
        private void RemoveEntityInternal(Entity entity)
        {
            Type entityType = entity.GetType();
            
            IEnumerable<Type> components = TypeCache.Get().GetComponentsOf(entityType);
            foreach (Type componentType in components)
            {
                if (entitiesByComponentType.TryGetValue(componentType, out List<EntId> entities))
                    entities.Remove(entity.ID);
            }
            
            DataContainersControllerImplementation dataContainersController = DataContainersControllerImplementation.GetInstance();
            IEnumerable<Type> componentDatasType = TypeCache.Get().GetComponentDatasOfEntityType(entity.GetType());
            foreach (Type componentDataType in componentDatasType)
            {
                object componentContainer = dataContainersController.GetComponentContainer(componentDataType);
                ((IGenericComponentContainer)componentContainer).RemoveComponent(entity.ID);
            }

            entitiesByID.Remove(entity.ID);
        }

        #endregion



    }

}