using System.Collections.Generic;
using Core.Utils.CachedDataStructures;

namespace Core.Systems
{
    public interface IModelSystem
    {
        public abstract void Update(float deltaTime);

        public bool Active { get; set; }
    }

    public abstract class BaseModelSystem<T> : IModelSystem where T : IEntity
    {
        private readonly List<T> destroyedEntities = new List<T>();
        private readonly Dictionary<int , T> entities = new Dictionary<int, T>();
        private int nextId = 0;
        
        protected virtual void OnEntityCreated(T entity) { }
        protected virtual void OnEntityDestroyed(T entity) { }
        
        public void AddEntity(T entity)
        {
            entity.ID = nextId++;
            entities.Add(entity.ID, entity);
            OnEntityCreated(entity);
        }
        
        public T GetEntity(int id)
        {
            if (entities.ContainsKey(id))
            {
                return entities[id];
            }
            else
            {
                return default(T);
            }
        }

        
        public void DestroyEntity(T entity)
        {
            OnEntityDestroyed(entity);
            entities.Remove(entity.ID);
            destroyedEntities.Add(entity);
        }

        public IEnumerable<T> Entities()
        {
            using CachedList<T> allEntities = ListCache<T>.Get();
            allEntities.AddRange(entities.Values);
            foreach (T entity in allEntities)
            {
                yield return entity;
            }
        }

        public IEnumerable<T> DeadEntities()
        {
            using CachedList<T> deadEntities = ListCache<T>.Get();
            deadEntities.AddRange(destroyedEntities);
            foreach (T entity in deadEntities)
            {
                yield return entity;
            }
        }


        public abstract void Update(float deltaTime);

        public bool Active { get; set; } = true;
    }

    public interface IEntity
    {
        public int ID { get; set; }
    }}