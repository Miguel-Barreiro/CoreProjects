using Core.Model;
using Core.Model.ModelSystems;
using Core.Systems;
using UnityEngine;
using Zenject;

#nullable enable

namespace Core.View
{
	// [OnDestroyProperties(LifetimePriority = SystemPriority.Early, UpdatePriority = SystemPriority.Early)]
	[OnDestroyComponentProperties(Priority = SystemPriority.Early)]
	[UpdateComponentProperties(Priority = SystemPriority.Early)]
	public sealed class PhisycsEntitiesUpdateViewSystem: ISystem, 
														UpdateComponents<PhysicsEntity2DData>, 
														OnCreateComponent<PhysicsEntity2DData>,
														OnDestroyComponent<PhysicsEntity2DData>
	
	{
		[Inject] private readonly ViewEntitiesContainer ViewEntitiesContainer = null!;
		[Inject] private readonly ComponentContainer<PhysicsEntity2DData> ComponentContainer = null!;
		
		public void OnCreateComponent(EntId newComponentId)
		{
			ref PhysicsEntity2DData newEntity = ref ComponentContainer.GetComponent(newComponentId);
			if (newEntity.Prefab == null)
			{
				Debug.LogError($"Prefab not found for entity {newEntity.GetType().Name}({newComponentId})");
				return;
			}
			Spawn(ref newEntity);
		}

		public void OnDestroyComponent(EntId destroyedComponentId)
		{
			EntityViewAtributes? entityViewAtributes = ViewEntitiesContainer.GetEntityViewAtributes(destroyedComponentId);
			if (entityViewAtributes == null)
			{
				Debug.LogError($"view for Entity I2DPhysicsEntityData with id {destroyedComponentId}() not found");
				return;
			}

			// Debug.Log($"destroying entity {entity.ID}({entity.GetType().Name})");
			ViewEntitiesContainer.Destroy(destroyedComponentId);
		}


		public void UpdateComponents(ComponentContainer<PhysicsEntity2DData> componentsContainer, float deltaTime)
		{
			componentsContainer.ResetIterator();
			while (componentsContainer.MoveNext())
			{
				ref PhysicsEntity2DData entity = ref componentsContainer.GetCurrent();

				EntityViewAtributes? viewAtributes = ViewEntitiesContainer.GetEntityViewAtributes(entity.ID);
				if (viewAtributes == null || viewAtributes.GameObject== null)
				{
					Spawn(ref entity);
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
		}

		private void Spawn(ref PhysicsEntity2DData newEntity)
		{
			GameObject? newGameObject = ViewEntitiesContainer.Spawn(newEntity.Prefab.gameObject, newEntity.ID);
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

		public bool Active { get; set; } = true;
		public SystemGroup Group { get; } = CoreSystemGroups.CorePhysicsEntitySystemGroup;
	}
	
}