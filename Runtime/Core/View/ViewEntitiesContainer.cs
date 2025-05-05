using System.Collections.Generic;
using Core.Model;
using UnityEngine;
using Zenject;

#nullable enable

namespace Core.View
{
	public interface IViewEntitiesContainer
	{
		public void Destroy(EntId entityID);
		public GameObject? Spawn(GameObject entityPrefab, IEntity entity);
		public EntityViewAtributes? GetEntityViewAtributes(EntId entId);
		public void SetEntityViewParent(EntId entId, Transform newParent);
	}

	public sealed class ViewEntitiesContainer : IViewEntitiesContainer
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
		
		public GameObject? Spawn(GameObject entityPrefab, IEntity entity)
		{
			if (entityPrefab == null)
			{
				Debug.Log($"No prefab set for entity {entity.GetType().Name}({entity.ID})");
				return null;
			}

			if (GameobjectsByEntityId.TryGetValue(entity.ID, out EntityViewAtributes entityViewAtributes) &&
				entityViewAtributes.GameObject != null)
			{
				return entityViewAtributes.GameObject;
			}

			GameObject newGameObject = genericGameObjectPool.GetGameObjectFromPrefab(entityPrefab)!;

			EntityViewAtributes entityAttributes = GetOrCreateEntityAttributes(entity);
			entityAttributes.GameObject = newGameObject;

			EntityView entityView = newGameObject.GetComponent<EntityView>();
			if(entityView != null)
				entityView.EntityID = entity.ID;

			return newGameObject;
		}

		public void SetEntityViewParent(EntId entId, Transform newParent)
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
		

		private EntityViewAtributes GetOrCreateEntityAttributes(IEntity entity)
		{
			if (!GameobjectsByEntityId.TryGetValue(entity.ID, out EntityViewAtributes entityViewAtributes))
			{
				entityViewAtributes = new EntityViewAtributes(entity.ID);
				GameobjectsByEntityId.Add(entity.ID, entityViewAtributes);
			}

			return entityViewAtributes;
		}

	}
}