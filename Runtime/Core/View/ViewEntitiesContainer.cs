using System.Collections.Generic;
using Core.Model;
using Core.Model.ModelSystems;
using UnityEngine;
using Zenject;

#nullable enable

namespace Core.View
{
	public interface IViewEntitiesContainer
	{
		public void Destroy(EntId entityID);
		public EntityViewAtributes? Spawn(GameObject entityPrefab, EntId entityID);
		public EntityViewAtributes? GetEntityViewAtributes(EntId entId);
		public void SetEntityViewParent(EntId entId, Transform newParent);
	}

	public sealed class ViewEntitiesContainer : IViewEntitiesContainer, IOnDestroyEntitySystem
	{
		[Inject] private readonly GenericGameObjectPool genericGameObjectPool = null!;
		
		private readonly Dictionary<EntId, EntityViewAtributes> GameobjectsByEntityId = new();
		
		public void Destroy(EntId entityID)
		{
			if (!GameobjectsByEntityId.TryGetValue(entityID, out EntityViewAtributes entityViewAtributes))
			{
				return;
			}

			if (entityViewAtributes.GameObject != null)
			{
				EntityView entityView = entityViewAtributes.GameObject.GetComponent<EntityView>();
				if(entityView != null)
					entityView.Reset();
				
				genericGameObjectPool.DestroyGameObject(entityViewAtributes.GameObject);
			}

			GameobjectsByEntityId.Remove(entityID);
		}
		
		public EntityViewAtributes? Spawn(GameObject entityPrefab, EntId entityID)
		{
			if (entityPrefab == null)
			{
				// Debug.Log($"No prefab set for entity {entity.GetType().Name}({entityID})");
				return null;
			}

			if (GameobjectsByEntityId.TryGetValue(entityID, out EntityViewAtributes entityViewAtributes) &&
				entityViewAtributes.GameObject != null)
			{
				return entityViewAtributes;
			}

			GameObject newGameObject = genericGameObjectPool.GetGameObjectFromPrefab(entityPrefab)!;

			entityViewAtributes = GetOrCreateEntityAttributes(entityID);
			entityViewAtributes.GameObject = newGameObject;

			EntityView entityView = newGameObject.GetComponent<EntityView>();
			if(entityView != null)
				entityView.EntityID = entityID;

			return entityViewAtributes;
		}

		public void SetEntityViewParent(EntId entId, Transform? newParent)
		{
			if (!GameobjectsByEntityId.TryGetValue(entId, out EntityViewAtributes entityViewAtributes))
			{
				return;
			}
			
			if (entityViewAtributes.GameObject != null) 
				entityViewAtributes.GameObject.transform.SetParent(newParent);
		}

		public EntityViewAtributes? GetEntityViewAtributes(EntId entId)
		{
			if (!GameobjectsByEntityId.TryGetValue(entId, out EntityViewAtributes entityViewAtributes))
			{
				return null;
			}

			return entityViewAtributes;
		}
		

		private EntityViewAtributes GetOrCreateEntityAttributes(EntId entityID)
		{
			if (!GameobjectsByEntityId.TryGetValue(entityID, out EntityViewAtributes entityViewAtributes))
			{
				entityViewAtributes = new EntityViewAtributes(entityID);
				GameobjectsByEntityId.Add(entityID, entityViewAtributes);
			}

			return entityViewAtributes;
		}

		public void OnDestroyEntity(EntId destroyedEntityId)
		{
			Destroy(destroyedEntityId);
		}
	}
}