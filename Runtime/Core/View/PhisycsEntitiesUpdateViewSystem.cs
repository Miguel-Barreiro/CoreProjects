using Core.Model;
using Core.Model.ModelSystems;
using UnityEngine;
using Zenject;

#nullable enable

namespace Core.View
{
	public sealed class PhisycsEntitiesUpdateViewSystem: ComponentSystem<I2DPhysicsEntity>
	{
		[Inject] private readonly ViewEntitiesContainer ViewEntitiesContainer = null!;
		
		public override void OnNew(I2DPhysicsEntity newEntity)
		{
			if (newEntity.Prefab == null)
			{
				Debug.LogError($"Prefab not found for entity {newEntity.GetType().Name}({newEntity.ID})");
				return;
			}

			GameObject? newGameObject = ViewEntitiesContainer.Spawn(newEntity.Prefab.gameObject, newEntity);
			if (newGameObject != null)
			{
				newGameObject.transform.position = new Vector3(newEntity.Position.x, newEntity.Position.y, 0);
			}

		}

		public override void OnDestroy(I2DPhysicsEntity entity)
		{
			EntityViewAtributes? entityViewAtributes = ViewEntitiesContainer.GetEntityViewAtributes(entity.ID);
			if (entityViewAtributes == null)
			{
				Debug.LogError($"view for Entity with id {entity.ID} not found");
				return;
			}

			ViewEntitiesContainer.Destroy(entity.ID);
		}


		public override void Update(I2DPhysicsEntity entity, float deltaTime)
		{
			EntityViewAtributes? viewAtributes = ViewEntitiesContainer.GetEntityViewAtributes(entity.ID);
			if (viewAtributes == null || viewAtributes.GameObject== null)
			{
				GameObject? newGameObject = ViewEntitiesContainer.Spawn(entity.Prefab.gameObject, entity);
				return;
			}

			GameObject entityGameobject = viewAtributes.GameObject;
			entity.Position = entityGameobject.transform.position;
		}
	}
}