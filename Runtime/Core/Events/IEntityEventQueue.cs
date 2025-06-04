using System;
using System.Collections.Generic;
using Core.Model;
using Core.Model.ModelSystems;
using Core.Utils.CachedDataStructures;
using UnityEngine;

namespace Core.Events
{

	public static class EntityEventQueue
	{
		public static TEntityEvent Execute<TEntityEvent>(EntId entId)
			where TEntityEvent : EntityEvent<TEntityEvent>, new()
		{
			return EntityEventQueuesContainer.Get().GetQueue<TEntityEvent>()?.Execute(entId);
		}
		
		public static void AddEntityEventListener<TEntityEvent>(EntId targetEntId, 
													IEntityEventQueue<TEntityEvent>.EntityEventListener callback)
			where TEntityEvent : EntityEvent<TEntityEvent>, new()
		{
			EntityEventQueuesContainer.Get().GetQueue<TEntityEvent>()?.AddEntityEventListener(targetEntId, callback);
		}
		
		public static void AddAllEntityEventListener<TEntityEvent>(IEntityEventQueue<TEntityEvent>.EntityEventListener callback)
			where TEntityEvent : EntityEvent<TEntityEvent>, new()
		{
			EntityEventQueuesContainer.Get().GetQueue<TEntityEvent>()?.AddAllEntitiesEventListener(callback);
		}

	}

	public interface IEntityEventQueue <TEntityEvent>
		where TEntityEvent : EntityEvent<TEntityEvent>, new()
	{
		public delegate void EntityEventListener(TEntityEvent entityEvent);
		
		public void AddEntityEventListener(EntId targetEntId, EntityEventListener callback);
		
		public void AddAllEntitiesEventListener(EntityEventListener callback);

		public void RemoveAllEntitiesEventListener(EntityEventListener callback);

		public void RemoveEntityEventListener(EntId targetEntId, EntityEventListener callback);

		public TEntityEvent Execute(EntId targetEntId);
		
		
	}

	public abstract class BaseEntityEventQueueImplementation : IOnUninstallSystem
	{
		internal abstract void RemoveEntity(EntId entId);

		internal abstract void ExecuteAllEvents();
		
		internal abstract int EventsCount();

		public abstract void OnUninstall(object system);
	}

	public sealed class EntityEventQueueImplementation<TEntityEvent> : BaseEntityEventQueueImplementation, 
																		IEntityEventQueue<TEntityEvent>
		where TEntityEvent : EntityEvent<TEntityEvent>, new()
	{
		// [Inject] private readonly ObjectBuilder ObjectBuilder = null!;
		
		private readonly List<IEntityEventQueue<TEntityEvent>.EntityEventListener> AllEntityEventListeners = new();
		private readonly Dictionary<EntId, List<IEntityEventQueue<TEntityEvent>.EntityEventListener>> EntityEventListeners = new();
		private readonly List<TEntityEvent> EntityEvents = new();
		
		
		internal override int EventsCount() => EntityEvents.Count;

		
		
		public override void OnUninstall(object system)
		{
			AllEntityEventListeners.RemoveAll(listener => listener.Target == system);
			foreach (List<IEntityEventQueue<TEntityEvent>.EntityEventListener> eventListeners in EntityEventListeners.Values)
				eventListeners.RemoveAll(listener => listener.Target == system);

		}

		internal override void RemoveEntity(EntId entId)
		{
			EntityEventListeners.Remove(entId);
		}
		
		
		public void AddEntityEventListener(EntId targetEntId, IEntityEventQueue<TEntityEvent>.EntityEventListener callback)
		{
			if (!EntityEventListeners.TryGetValue(targetEntId, out List<IEntityEventQueue<TEntityEvent>.EntityEventListener> eventListeners))
			{
				eventListeners = new();
				EntityEventListeners.Add(targetEntId, eventListeners);
			}
			eventListeners.Add(callback);
		}

		public void AddAllEntitiesEventListener(IEntityEventQueue<TEntityEvent>.EntityEventListener callback)
		{
			AllEntityEventListeners.Add(callback);
		}

		public void RemoveAllEntitiesEventListener(IEntityEventQueue<TEntityEvent>.EntityEventListener callback)
		{
			AllEntityEventListeners.Remove(callback);
		}

		public void RemoveEntityEventListener(EntId targetEntId, IEntityEventQueue<TEntityEvent>.EntityEventListener callback)
		{
			if (!EntityEventListeners.TryGetValue(targetEntId, out List<IEntityEventQueue<TEntityEvent>.EntityEventListener> eventListeners))
				return;
			eventListeners.Remove(callback);
		}


		public TEntityEvent Execute(EntId targetEntId)
		{
			TEntityEvent newEvent = EntityEvent<TEntityEvent>.Pool.Spawn();
			newEvent.SetTargetEntityID(targetEntId);
			EntityEvents.Add(newEvent);
			
			// Type eventType = typeof(TEntityEvent);
			// ObjectBuilder.Inject(newEvent);
			// TypeCache.EventAttributes? attributes = TypeCache.Get().GetEventAttributes(eventType);
			// EventOrder newEventOrder = attributes?.EventOrder ?? EventOrder.Default;
			// this.EventQueueByType[newEventOrder][eventType].Add(newEvent);
			return newEvent;
		}
		
		internal override void ExecuteAllEvents()
		{
			using CachedList<TEntityEvent> toProcess = ListCache<TEntityEvent>.Get();
			toProcess.AddRange(EntityEvents);
			EntityEvents.Clear();
			
			foreach (TEntityEvent entityEvent in toProcess)
			{
				if (EntityEventListeners.TryGetValue(entityEvent.EntityID, 
													out List<IEntityEventQueue<TEntityEvent>.EntityEventListener> eventListeners))
				{
					foreach (IEntityEventQueue<TEntityEvent>.EntityEventListener eventListener in eventListeners)
						eventListener.Invoke(entityEvent);
				}

				foreach (IEntityEventQueue<TEntityEvent>.EntityEventListener eventListener in AllEntityEventListeners)
					eventListener.Invoke(entityEvent);
				
				entityEvent.Dispose();
			}
			
		}
	}

	public sealed class EntityEventQueuesContainer : IOnDestroyEntitySystem
	{
		private Dictionary<Type, BaseEntityEventQueueImplementation> _entityEventQueuesByEventType = new();

		private static EntityEventQueuesContainer _instance = null!;
		internal static EntityEventQueuesContainer Get()
		{
			if(_instance == null)
				_instance = new EntityEventQueuesContainer();

			return _instance;
		}

		private EntityEventQueuesContainer()
		{
			IEnumerable<Type> allEntityEventTypes = TypeCache.Get().GetAllEntityEventTypes();
			
			foreach (Type entityEventType in allEntityEventTypes)
			{
				var containerType = typeof(EntityEventQueueImplementation<>).MakeGenericType(entityEventType);
				object newEntityEventContainer = Activator.CreateInstance(containerType);
				
				_entityEventQueuesByEventType[entityEventType] = (BaseEntityEventQueueImplementation)newEntityEventContainer;
			}
		}
		
		internal IEnumerable<KeyValuePair<Type,BaseEntityEventQueueImplementation>> GetAllEntityEventQueues()
		{
			foreach (KeyValuePair<Type,BaseEntityEventQueueImplementation> keyValuePair in _entityEventQueuesByEventType)
				yield return keyValuePair;
		}

		internal void ClearAll()
		{
			_entityEventQueuesByEventType.Clear();
		}
		
		internal void ExecuteAllEntityEvents()
		{
			bool hasEvents = true;
			while (hasEvents)
			{
				hasEvents = false;
				foreach (BaseEntityEventQueueImplementation entityEventQueue in _entityEventQueuesByEventType.Values)
				{
					if (entityEventQueue.EventsCount() > 0)
					{
						entityEventQueue.ExecuteAllEvents();
						hasEvents = true;	
					}
				}
			}
		}

		public EntityEventQueueImplementation<TEntityEvent> GetQueue<TEntityEvent>()
			where TEntityEvent : EntityEvent<TEntityEvent>, new()
		{
			Type entityEventType = typeof(TEntityEvent);
			if(!_entityEventQueuesByEventType.TryGetValue(entityEventType, 
														out BaseEntityEventQueueImplementation? entityEventQueue))
			{
				Debug.LogError($"No entity event queue found for type {entityEventType.Name}"); 
				return null;
			}
			
			return (EntityEventQueueImplementation<TEntityEvent>)entityEventQueue;
		}

		public void OnDestroyEntity(EntId destroyedEntityId) { 
			foreach (BaseEntityEventQueueImplementation entityEventQueue in _entityEventQueuesByEventType.Values)
				entityEventQueue.RemoveEntity(destroyedEntityId);
		}
		
	}
	
	

}