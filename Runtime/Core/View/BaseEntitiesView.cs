using System;
using System.Collections.Generic;
using Core.View;
using Core.Systems;
using Core.Zenject.Source.Internal;
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
            private Dictionary<Type, Component> componentCache = new Dictionary<Type, Component>();
            public GameObject GameObject { get; private set; }
            
            public EntityViewAtributes(GameObject gameObject)
            {
                GameObject = gameObject;
            }
            public T GetComponent<T>() where T : Component
            {
                Type type = typeof(T);
                if (componentCache.TryGetValue(type, out Component component))
                {
                    return (T) component;
                }
                else
                {
                    T newComponent = GameObject.GetComponent<T>();
                    if (newComponent == null)
                    {
                        newComponent = GameObject.GetComponentInChildren<T>();
                    }
                    if (newComponent == null)
                    {
                        Debug.LogError($"component {type} not found in {GameObject.name} in view system {typeof(TSystem).PrettyName()}");
                        return null;
                    }

                    componentCache.Add(type, newComponent);
                    return newComponent;
                }
            }
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
                    Destroy(entityViewAtributes.GameObject);
                }
            }
        }
        

        public bool Active { get; set; }
    }
}