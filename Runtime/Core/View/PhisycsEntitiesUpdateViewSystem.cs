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
														UpdateComponents<Physics2DComponentData>, 
														OnCreateComponent<Physics2DComponentData>,
														OnDestroyComponent<Physics2DComponentData>
	
	{
		[Inject] private readonly ViewEntitiesContainer ViewEntitiesContainer = null!;
		[Inject] private readonly ComponentContainer<Physics2DComponentData> ComponentContainer = null!;
		
		public void OnCreateComponent(EntId newComponentId)
		{
			ref Physics2DComponentData newComponent = ref ComponentContainer.GetComponent(newComponentId);
			if (newComponent.Prefab == null)
			{
				Debug.LogError($"Prefab not found for entity {newComponent.GetType().Name}({newComponentId})");
				return;
			}
			Spawn(ref newComponent);
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


		public void UpdateComponents(ComponentContainer<Physics2DComponentData> componentsContainer, float deltaTime)
		{
			componentsContainer.ResetIterator();
			while (componentsContainer.MoveNext())
			{
				ref Physics2DComponentData component = ref componentsContainer.GetCurrent();

				EntityViewAtributes? viewAtributes = ViewEntitiesContainer.GetEntityViewAtributes(component.ID);
				if (viewAtributes == null || viewAtributes.GameObject== null)
				{
					Spawn(ref component);
					return;
				}

				GameObject entityGameobject = viewAtributes.GameObject;
		
				switch (component.Rigidbody2D.bodyType)
				{
					case RigidbodyType2D.Dynamic:
						component.Position = entityGameobject.transform.position;
						break;
					case RigidbodyType2D.Kinematic:
						entityGameobject.transform.position = new Vector3(component.Position.x, component.Position.y, 0);
						break;
					case RigidbodyType2D.Static:
						break;
				}
				
			}
		}

		private void Spawn(ref Physics2DComponentData newComponent)
		{
			GameObject? newGameObject = ViewEntitiesContainer.Spawn(newComponent.Prefab.gameObject, newComponent.ID);
			if (newGameObject == null)
			{
				Debug.LogError($"no Gameobject was able to be created for physics entity with prefab {newComponent.Prefab.name}");
				return;
			}

			newGameObject.transform.position = new Vector3(newComponent.Position.x, newComponent.Position.y, 0);

			Rigidbody2D rigidbody2D = newGameObject.GetComponent<Rigidbody2D>();
			if(rigidbody2D == null)
			{
				Debug.LogError($"no rigidBody found for physics entity with prefab {newComponent.Prefab.name}"); 
				return;
			}
			
			newComponent.Rigidbody2D = rigidbody2D;
		}

		public bool Active { get; set; } = true;
		public SystemGroup Group { get; } = CoreSystemGroups.CorePhysicsEntitySystemGroup;
	}
	
}