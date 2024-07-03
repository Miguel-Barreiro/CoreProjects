using System.Collections.Generic;
using Core.Model;
using Core.Systems;
using UnityEngine;
using Zenject;

namespace Core.View
{
    public abstract class BaseEntitiesView<TEntity>: EntitySystem<TEntity> 
        where TEntity : BaseEntity, IPositionEntity
    {

        [Inject] private readonly GenericGameObjectPool genericGameObjectPool = null!;
        
        protected readonly Dictionary<EntId, EntityViewAtributes> GameobjectsByEntityId = new();
        
        protected virtual void UpdateEntity(TEntity entity, EntityViewAtributes entityViewAtributes) { }
        protected virtual void OnSpawn(TEntity entity, GameObject newGameObject) { }
        
        
        protected class EntityViewAtributes
        {
            public EntityViewAtributes(GameObject newGameObject)
            {
                GameObject = newGameObject;
            }

            public GameObject GameObject { get; private set; }
        }
        
        
        protected virtual GameObject Spawn(TEntity entity)
        {
            GameObject newGameObject = genericGameObjectPool.GetGameObjectFromPrefab(entity.Prefab);
            newGameObject.transform.position = new Vector3(entity.Position.x, entity.Position.y, 0);

            GameobjectsByEntityId.Add(entity.ID, new EntityViewAtributes(newGameObject) );

            OnSpawn(entity, newGameObject);
            
            return newGameObject;
        }


        public override void OnNew(TEntity newEntity)
        {
            if (newEntity.Prefab == null)
            {
                Debug.LogError($"Prefab not found for entity {typeof(TEntity).Name}({newEntity.ID})");
                return;
            }

            Spawn(newEntity);
        }

        public override void OnDestroy(TEntity entity)
        {
            if (!GameobjectsByEntityId.ContainsKey(entity.ID))
            {
                Debug.LogError($"Entity with id {entity.ID} not found");
                return;
            }
            EntityViewAtributes entityViewAtributes = GameobjectsByEntityId[entity.ID];
            genericGameObjectPool.DestroyGameObject(entityViewAtributes.GameObject);
            GameobjectsByEntityId.Remove(entity.ID);
        }


        public override void Update(TEntity entity, float deltaTime)
        {
            if (!GameobjectsByEntityId.ContainsKey(entity.ID))
            {
                Spawn(entity);
                return;
            }
            
            EntityViewAtributes entityViewAtributes = GameobjectsByEntityId[entity.ID];
            GameObject entityGameobject = entityViewAtributes.GameObject;
            entityGameobject.transform.position = new Vector3(entity.Position.x, entity.Position.y, 0);
            UpdateEntity(entity, entityViewAtributes);
        }

    }
}