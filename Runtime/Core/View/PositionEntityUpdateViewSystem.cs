using Core.Model;
using Core.Model.ModelSystems;
using Core.Systems;
using UnityEngine;
using Zenject;

#nullable enable

namespace Core.View
{
    
    public sealed class PositionEntityUpdateViewSystem : ComponentSystem<IKineticEntity>
    {
        [Inject] private readonly ViewEntitiesContainer ViewEntitiesContainer = null!;

        public override void OnNew(IKineticEntity newEntity)
        {
            if (newEntity.Prefab == null)
            {
                Debug.LogError($"Prefab not found for entity {newEntity.GetType().Name}({newEntity.ID})");
                return;
            }

            GameObject? newGameObject = ViewEntitiesContainer.Spawn(newEntity.Prefab, newEntity);
            if (newGameObject != null)
            {
                newGameObject.transform.position = new Vector3(newEntity.Position.x, newEntity.Position.y, 0);
            }

        }

        public override void OnDestroy(IKineticEntity entity)
        {
            EntityViewAtributes? entityViewAtributes = ViewEntitiesContainer.GetEntityViewAtributes(entity.ID);
            if (entityViewAtributes == null)
            {
                Debug.LogError($"view for Entity with id {entity.ID} not found");
                return;
            }

            ViewEntitiesContainer.Destroy(entity.ID);
        }


        public override void Update(IKineticEntity entity, float deltaTime)
        {
            EntityViewAtributes? viewAtributes = ViewEntitiesContainer.GetEntityViewAtributes(entity.ID);
            if (viewAtributes == null || viewAtributes.GameObject== null)
            {
                GameObject? newGameObject = ViewEntitiesContainer.Spawn(entity.Prefab, entity);
                return;
            }

            GameObject entityGameobject = viewAtributes.GameObject;
            entityGameobject.transform.position = new Vector3(entity.Position.x, entity.Position.y, 0);
        }

        public override SystemGroup Group { get; } = CoreSystemGroups.CoreViewEntitySystemGroup;
    }
}