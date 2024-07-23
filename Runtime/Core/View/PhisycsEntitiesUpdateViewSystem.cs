using Core.Model;
using Core.Model.ModelSystems;
using Core.Systems;
using UnityEngine;
using Zenject;

#nullable enable

namespace Core.View
{
	[EntitySystemPropertiesAttribute(SystemPriority = SystemPriority.Early)]
	public sealed class PhisycsEntitiesUpdateViewSystem: ComponentSystem<I2DPhysicsEntity>
	{
		[Inject] private readonly ViewEntitiesContainer ViewEntitiesContainer = null!;
		
		public override void OnNew(I2DPhysicsEntity newEntity)
		{
			Debug.Log($"on NEW physics entity: EARLY");
			
			if (newEntity.Prefab == null)
			{
				Debug.LogError($"Prefab not found for entity {newEntity.GetType().Name}({newEntity.ID})");
				return;
			}
			Spawn(newEntity);
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
			Debug.Log($"on update physics entity: EARLY");
			
			EntityViewAtributes? viewAtributes = ViewEntitiesContainer.GetEntityViewAtributes(entity.ID);
			if (viewAtributes == null || viewAtributes.GameObject== null)
			{
				Spawn(entity);
				return;
			}

			GameObject entityGameobject = viewAtributes.GameObject;
			entity.Position = entityGameobject.transform.position;
		}

		private void Spawn(I2DPhysicsEntity newEntity)
		{
			GameObject? newGameObject = ViewEntitiesContainer.Spawn(newEntity.Prefab.gameObject, newEntity);
			if (newGameObject != null)
			{
				newGameObject.transform.position = new Vector3(newEntity.Position.x, newEntity.Position.y, 0);
			}

			Rigidbody2D rigidbody2D = newGameObject.GetComponent<Rigidbody2D>();
			if(rigidbody2D == null)
			{
				Debug.LogError($"no rigidBody found for physics entity with prefab {newEntity.Prefab.name}"); 
				return;
			}
			
			newEntity.Rigidbody2D = rigidbody2D;
		}
	}
	
}