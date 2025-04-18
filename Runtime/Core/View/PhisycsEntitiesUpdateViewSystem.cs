using Core.Model;
using Core.Model.ModelSystems;
using Core.Systems;
using UnityEngine;
using Zenject;

#nullable enable

namespace Core.View
{
	[EntitySystemPropertiesAttribute(LifetimePriority = SystemPriority.Early, UpdatePriority = SystemPriority.Early)]
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
			Spawn(newEntity);
		}

		public override void OnDestroy(I2DPhysicsEntity entity)
		{
			EntityViewAtributes? entityViewAtributes = ViewEntitiesContainer.GetEntityViewAtributes(entity.ID);
			if (entityViewAtributes == null)
			{
				Debug.LogError($"view for Entity with id {entity.ID}({entity.GetType().Name}) not found");
				return;
			}

			// Debug.Log($"destroying entity {entity.ID}({entity.GetType().Name})");
			ViewEntitiesContainer.Destroy(entity.ID);
		}


		public override void Update(I2DPhysicsEntity entity, float deltaTime)
		{
			EntityViewAtributes? viewAtributes = ViewEntitiesContainer.GetEntityViewAtributes(entity.ID);
			if (viewAtributes == null || viewAtributes.GameObject== null)
			{
				Spawn(entity);
				return;
			}

			GameObject entityGameobject = viewAtributes.GameObject;
			
			switch (entity.Rigidbody2D.bodyType)
			{
				case RigidbodyType2D.Dynamic:
					entity.Position = entityGameobject.transform.position;
					break;
				case RigidbodyType2D.Kinematic:
					entityGameobject.transform.position = new Vector3(entity.Position.x, entity.Position.y, 0);
					break;
				case RigidbodyType2D.Static:
					break;
			}
		}

		private void Spawn(I2DPhysicsEntity newEntity)
		{
			GameObject? newGameObject = ViewEntitiesContainer.Spawn(newEntity.Prefab.gameObject, newEntity);
			if (newGameObject == null)
			{
				Debug.LogError($"no Gameobject was able to be created for physics entity with prefab {newEntity.Prefab.name}");
				return;
			}

			newGameObject.transform.position = new Vector3(newEntity.Position.x, newEntity.Position.y, 0);

			Rigidbody2D rigidbody2D = newGameObject.GetComponent<Rigidbody2D>();
			if(rigidbody2D == null)
			{
				Debug.LogError($"no rigidBody found for physics entity with prefab {newEntity.Prefab.name}"); 
				return;
			}
			
			newEntity.Rigidbody2D = rigidbody2D;
		}

		public override SystemGroup Group { get; } = CoreSystemGroups.CorePhysicsEntitySystemGroup;
	}
	
}