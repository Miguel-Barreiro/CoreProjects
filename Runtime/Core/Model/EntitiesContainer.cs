using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Core.Model.ModelSystems;
using Core.Systems;
using Unity.Collections.LowLevel.Unsafe;

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

        public void AddTag(EntId id, ulong tag);
        public void RemoveTag(EntId id, ulong tag);
        public bool HasAnyTag(EntId id, params ulong[] tags);
        public bool HasAnyTag(EntId id, ulong mask);
        public bool HasAllTags(EntId id, ulong mask);
        public bool HasNoTags(EntId id, ulong mask);
        public void ToggleTag(EntId id, ulong tag);
        public void SetTag(EntId id, ulong tag, bool enabled);
        public ulong GetTags(EntId id);

        public void AddTag<TEnum>(EntId id, TEnum tags) where TEnum : unmanaged, Enum;
        public void RemoveTag<TEnum>(EntId id, TEnum tags) where TEnum : unmanaged, Enum;
        public void ToggleTag<TEnum>(EntId id, TEnum tags) where TEnum : unmanaged, Enum;
        public void SetTag<TEnum>(EntId id, TEnum tags, bool enabled) where TEnum : unmanaged, Enum;
        public bool HasAnyTag<TEnum>(EntId id, TEnum tags) where TEnum : unmanaged, Enum;
        public bool HasAllTags<TEnum>(EntId id, TEnum tags) where TEnum : unmanaged, Enum;
        public bool HasNoTags<TEnum>(EntId id, TEnum tags) where TEnum : unmanaged, Enum;
        public TEnum GetTags<TEnum>(EntId id) where TEnum : unmanaged, Enum;
    }

    public class EntitiesContainer : IInitSystem, IEntitiesContainer
    {

        // ENTITY LIFETIME MANAGEMENT
        private readonly Dictionary<EntId, Entity> entitiesByID = new ();

        // COMPONENT CACHING
        private readonly Dictionary<Type, List<EntId>> entitiesByComponentType = new ();
        
        //EVENT PROCESSING
        private readonly HashSet<EntId> destroyedEntities = new ();
        private readonly Dictionary<EntId,Entity> newEntities = new ();
        
        
        // ENTITY TAGGING
        private readonly Dictionary<EntId, ulong> EntityTags = new();
        
        private uint nextEntityID = 1;

        private static EntitiesContainer? instance = null!;
        

        public static void DestroyEntity(EntId entityId) { instance!.destroyedEntities.Add(entityId); }


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
            instance.EntityTags.Clear();
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
           return newEntities[entID];
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
            foreach (Entity newEntity in newEntities.Values)
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
                if (destroyedEntity == null)
                {
                    if (newEntities.TryGetValue(destroyedEntityId, out destroyedEntity))
                        yield return destroyedEntity;
                    Entity? entityFromFlushedNewEntities = GetEntityFromFlushedNewEntities(destroyedEntityId);
                    if(entityFromFlushedNewEntities != null)
                        yield return entityFromFlushedNewEntities;
                } else
                {
                    yield return destroyedEntity;
                }
            }
        }

        
        private readonly HashSet<Entity> FlushedDeadEntities = new();
        private readonly HashSet<Entity> FlushedNewEntities = new();
        
        private Entity? GetEntityFromFlushedNewEntities(EntId entID)
        {
            foreach (Entity flushedNewEntity in FlushedNewEntities)
            {
                if (flushedNewEntity.ID == entID)
                    return flushedNewEntity;
            }
            return null;
        }
        
        internal void FlushCurrentDestroyedEntities()
        {
            IEnumerable<Entity> allDestroyedEntities = GetAllDestroyedEntities();
            foreach (Entity destroyedEntity in allDestroyedEntities)
                FlushedDeadEntities.Add(destroyedEntity);
            
            destroyedEntities.Clear();
        }
        internal void FlushCurrentNewEntities()
        {
            foreach (Entity newEntity in newEntities.Values)
            {
                if (!destroyedEntities.Contains(newEntity.ID) &&
                    !FlushedDeadEntities.Contains(newEntity))
                    FlushedNewEntities.Add(newEntity);
                else
                {
                    // Destroyed before it was ever upgraded. Once newEntities is cleared below,
                    // FlushCurrentDestroyedEntities can no longer find it, so queue it as dead here.
                    // Its component data and tags are then released at the end of the frame by
                    // RemoveEntityInternal, after the OnDestroy systems have had a chance to read them.
                    FlushedDeadEntities.Add(newEntity);
                }

            }
            newEntities.Clear();
        }
        


        public void FlushEntities()
        {
            FlushCurrentNewEntities();
            FlushCurrentDestroyedEntities();
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
            {
                if(!FlushedDeadEntities.Contains(newEntity) )
                    AddEntityInternal(newEntity);
            }
            
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
            EntId newEntityID = entity.ID;
            instance!.newEntities.Add(newEntityID,entity);

            instance!.EntityTags[newEntityID] = entity.GetStartTags();
            
            DataContainersControllerImplementation dataContainersController = DataContainersControllerImplementation.GetInstance();
            IEnumerable<Type> componentDatasType = TypeCache.Get().GetComponentDatasOfEntityType(entity.GetType());
            foreach (Type componentDataType in componentDatasType)
            {
                object componentContainer = dataContainersController.GetComponentContainer(componentDataType);
                ((IGenericComponentContainer)componentContainer).SetupComponent(newEntityID);
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
                yield return (entity! as TEntity)!;
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
            // Keep any tags added while the entity was still new, and merge in its start tags.
            // EntityTags[entity.ID] = EntityTags.GetValueOrDefault(entity.ID) | entity.GetStartTags();
            //
            // DataContainersControllerImplementation dataContainersController = DataContainersControllerImplementation.GetInstance();
            // IEnumerable<Type> componentDatasType = TypeCache.Get().GetComponentDatasOfEntityType(entity.GetType());
            // foreach (Type componentDataType in componentDatasType)
            // {
            //     object componentContainer = dataContainersController.GetComponentContainer(componentDataType);
            //     ((IGenericComponentContainer)componentContainer).UpgradeNewComponent(entity.ID);
            // }

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

            RemoveComponentDatas(entity);

            entitiesByID.Remove(entity.ID);
            EntityTags.Remove(entity.ID);
        }

        private static void RemoveComponentDatas(Entity entity)
        {
            DataContainersControllerImplementation dataContainersController = DataContainersControllerImplementation.GetInstance();
            IEnumerable<Type> componentDatasType = TypeCache.Get().GetComponentDatasOfEntityType(entity.GetType());
            foreach (Type componentDataType in componentDatasType)
            {
                object componentContainer = dataContainersController.GetComponentContainer(componentDataType);
                ((IGenericComponentContainer)componentContainer).RemoveComponent(entity.ID);
            }
        }

        #endregion


        #region Entity Tagging

        /// <summary>Adds the bits in <paramref name="tag"/> to the entity. Works on new (not yet processed) entities too. Unknown ids are ignored.</summary>
        public void AddTag(EntId id, ulong tag)
        {
            if (!EntityExists(id))
                return;

            EntityTags[id] = EntityTags.GetValueOrDefault(id) | tag;
        }

        /// <summary>Clears the bits in <paramref name="tag"/> from the entity. Unknown ids are ignored.</summary>
        public void RemoveTag(EntId id, ulong tag)
        {
            if (!EntityTags.TryGetValue(id, out ulong current))
                return;

            EntityTags[id] = current & ~tag;
        }

        /// <summary>True if the entity has at least one of the given tags.</summary>
        public bool HasAnyTag(EntId id, params ulong[] tags)
        {
            if (tags == null || tags.Length == 0)
                return false;

            ulong mask = 0;
            for (int i = 0; i < tags.Length; i++)
                mask |= tags[i];

            return HasAnyTag(id, mask);
        }

        /// <summary>True if the entity has at least one of the bits set in <paramref name="mask"/>.</summary>
        public bool HasAnyTag(EntId id, ulong mask)
        {
            return EntityTags.TryGetValue(id, out ulong current) && (current & mask) != 0;
        }

        /// <summary>True if the entity has every bit set in <paramref name="mask"/>. Unknown ids return false.</summary>
        public bool HasAllTags(EntId id, ulong mask)
        {
            return EntityTags.TryGetValue(id, out ulong current) && (current & mask) == mask;
        }

        /// <summary>True if the entity has none of the bits set in <paramref name="mask"/>. Unknown ids return true.</summary>
        public bool HasNoTags(EntId id, ulong mask)
        {
            return (EntityTags.GetValueOrDefault(id) & mask) == 0;
        }

        /// <summary>Flips the bits in <paramref name="tag"/> on the entity. Unknown ids are ignored.</summary>
        public void ToggleTag(EntId id, ulong tag)
        {
            if (!EntityExists(id))
                return;

            EntityTags[id] = EntityTags.GetValueOrDefault(id) ^ tag;
        }

        /// <summary>Adds or clears the bits in <paramref name="tag"/> depending on <paramref name="enabled"/>.</summary>
        public void SetTag(EntId id, ulong tag, bool enabled)
        {
            if (enabled) AddTag(id, tag);
            else RemoveTag(id, tag);
        }

        /// <summary>The entity's full tag mask, or 0 for unknown ids.</summary>
        public ulong GetTags(EntId id)
        {
            return EntityTags.GetValueOrDefault(id);
        }

        // ---- project-defined flags enum API ----
        // Projects can define their tags as a flags enum, e.g.
        //   [Flags] public enum HexObjectType : ulong { None = 0, Unit = 1 << 0, City = 1 << 1, Tile = 1 << 2 }
        //   entitiesContainer.AddTag(id, HexObjectType.Unit);
        //   if (entitiesContainer.HasAnyTag(id, HexObjectType.Unit | HexObjectType.City)) { ... }
        // Up to 64 distinct tags are supported. Enum conversion is allocation free.

        public void AddTag<TEnum>(EntId id, TEnum tags) where TEnum : unmanaged, Enum => AddTag(id, ToMask(tags));
        public void RemoveTag<TEnum>(EntId id, TEnum tags) where TEnum : unmanaged, Enum => RemoveTag(id, ToMask(tags));
        public void ToggleTag<TEnum>(EntId id, TEnum tags) where TEnum : unmanaged, Enum => ToggleTag(id, ToMask(tags));
        public void SetTag<TEnum>(EntId id, TEnum tags, bool enabled) where TEnum : unmanaged, Enum => SetTag(id, ToMask(tags), enabled);
        public bool HasAnyTag<TEnum>(EntId id, TEnum tags) where TEnum : unmanaged, Enum => HasAnyTag(id, ToMask(tags));
        public bool HasAllTags<TEnum>(EntId id, TEnum tags) where TEnum : unmanaged, Enum => HasAllTags(id, ToMask(tags));
        public bool HasNoTags<TEnum>(EntId id, TEnum tags) where TEnum : unmanaged, Enum => HasNoTags(id, ToMask(tags));

        /// <summary>Reads the entity's tags as the project's flags enum.</summary>
        public TEnum GetTags<TEnum>(EntId id) where TEnum : unmanaged, Enum => FromMask<TEnum>(GetTags(id));

        /// <summary>Converts a flags enum value (any underlying integer type) to a 64-bit mask without boxing.</summary>
        public static ulong ToMask<TEnum>(TEnum value) where TEnum : unmanaged, Enum
        {
            switch (UnsafeUtility.SizeOf<TEnum>())
            {
                case 1: return UnsafeUtility.As<TEnum, byte>(ref value);
                case 2: return UnsafeUtility.As<TEnum, ushort>(ref value);
                case 4: return UnsafeUtility.As<TEnum, uint>(ref value);
                case 8: return UnsafeUtility.As<TEnum, ulong>(ref value);
                default: throw new ArgumentException($"Unsupported enum size for {typeof(TEnum)}");
            }
        }

        /// <summary>Converts a 64-bit mask back to a flags enum, truncating to the enum's underlying size.</summary>
        public static TEnum FromMask<TEnum>(ulong mask) where TEnum : unmanaged, Enum
        {
            switch (UnsafeUtility.SizeOf<TEnum>())
            {
                case 1: { byte v = (byte)mask; return UnsafeUtility.As<byte, TEnum>(ref v); }
                case 2: { ushort v = (ushort)mask; return UnsafeUtility.As<ushort, TEnum>(ref v); }
                case 4: { uint v = (uint)mask; return UnsafeUtility.As<uint, TEnum>(ref v); }
                case 8: return UnsafeUtility.As<ulong, TEnum>(ref mask);
                default: throw new ArgumentException($"Unsupported enum size for {typeof(TEnum)}");
            }
        }

        private bool EntityExists(EntId id)
        {
            return entitiesByID.ContainsKey(id) || newEntities.ContainsKey(id) || GetEntityFromFlushedNewEntities(id) != null;
        }

        #endregion

    }

}