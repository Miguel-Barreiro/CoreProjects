using System.Collections.Generic;
using Core.Systems;
using UnityEngine;
using Zenject;

namespace Core.View
{
    public abstract class BaseEntitiesView<TSystem, T>: MonoBehaviour, IUpdateSystem 
        where T : IPositionEntity
        where TSystem : BaseModelSystem<T>
    {

        [Inject] private readonly TSystem System = null!;
        [Inject] private readonly GenericGameObjectPool GenericGameObjectPool = null!;
        
        [SerializeField]
        protected Dictionary<int, EntityViewAtributes> GameobjectsByEntityId = new Dictionary<int, EntityViewAtributes>();

        protected class EntityViewAtributes
        {
            public EntityViewAtributes(GameObject newGameObject)
            {
                GameObject = newGameObject;
            }

            public GameObject GameObject { get; private set; }
        }
            
            
        
        protected virtual void UpdateEntity(T entity, EntityViewAtributes entityViewAtributes) { }

        protected virtual void OnSpawn(T entity, GameObject newGameObject) { }
        
        protected virtual GameObject Spawn(T entity)
        {
            GameObject newGameObject = GenericGameObjectPool.GetGameObjectFromPrefab(entity.Prefab, transform);
            newGameObject.transform.position = new Vector3(entity.Position.x, entity.Position.y, 0);

            GameobjectsByEntityId.Add(entity.ID, new EntityViewAtributes(newGameObject) );
            newGameObject.transform.SetParent(transform);

            OnSpawn(entity, newGameObject);
            
            return newGameObject;
        }

        public void Update()
        {
            IEnumerable<T> entities = System.Entities();
            
            foreach (T entitiy in entities)
            {
                if (!GameobjectsByEntityId.ContainsKey(entitiy.ID))
                {
                    Spawn(entitiy);
                    continue;
                }

                EntityViewAtributes entityViewAtributes = GameobjectsByEntityId[entitiy.ID];
                GameObject entityGameobject = entityViewAtributes.GameObject;
                entityGameobject.transform.position = new Vector3(entitiy.Position.x, entitiy.Position.y, 0);
                UpdateEntity(entitiy, entityViewAtributes);
            }

            IEnumerable<T> deadEntities = System.DeadEntities();
            foreach (T deadEntity in deadEntities)
            {
                if (GameobjectsByEntityId.TryGetValue(deadEntity.ID, out EntityViewAtributes entityViewAtributes))
                {
                    GameobjectsByEntityId.Remove(deadEntity.ID);
                    GenericGameObjectPool.DestroyGameObject(entityViewAtributes.GameObject);
                }
            }
        }
        

        public bool Active { get; set; }
    }
}