using System;
using System.Collections.Generic;
using Core.Model;
using Core.Utils.CachedDataStructures;
using Zenject;

namespace Core.Events
{
	public interface EntityEventQueue <TEntityEvent>
		where TEntityEvent : EntityEvent<TEntityEvent>, new()
	{
		public delegate void EntityEventListener(TEntityEvent entityEvent);
		
		public void AddEntityEventListener(EntId targetEntId, EntityEventListener callback);
		
		public void AddAllEntitiesEventListener(EntityEventListener callback);

		public void RemoveAllEntitiesEventListener(EntityEventListener callback);

		public void RemoveEntityEventListener(EntId targetEntId, EntityEventListener callback);

		public TEntityEvent Execute(EntId targetEntId);
		
	}

	public abstract class BaseEntityEventQueueImplementation
	{
		internal abstract void RemoveEntity(EntId entId);

		internal abstract void ExecuteAllEvents();
		
		internal abstract int EventsCount();  

	}

	public sealed class EntityEventQueueImplementation<TEntityEvent> : BaseEntityEventQueueImplementation, 
																		EntityEventQueue<TEntityEvent>
		where TEntityEvent : EntityEvent<TEntityEvent>, new()
	{
		// [Inject] private readonly ObjectBuilder ObjectBuilder = null!;
		
		private readonly List<EntityEventQueue<TEntityEvent>.EntityEventListener> AllEntityEventListeners = new();
		private readonly Dictionary<EntId, List<EntityEventQueue<TEntityEvent>.EntityEventListener>> EntityEventListeners = new();
		private readonly List<TEntityEvent> EntityEvents = new();
		
		
		internal override int EventsCount() => EntityEvents.Count;
		
		internal override void RemoveEntity(EntId entId)
		{
			EntityEventListeners.Remove(entId);
		}

		
		
		public void AddEntityEventListener(EntId targetEntId, EntityEventQueue<TEntityEvent>.EntityEventListener callback)
		{
			if (!EntityEventListeners.TryGetValue(targetEntId, out List<EntityEventQueue<TEntityEvent>.EntityEventListener> eventListeners))
			{
				eventListeners = new();
				EntityEventListeners.Add(targetEntId, eventListeners);
			}
			eventListeners.Add(callback);
		}

		public void AddAllEntitiesEventListener(EntityEventQueue<TEntityEvent>.EntityEventListener callback)
		{
			AllEntityEventListeners.Add(callback);
		}

		public void RemoveAllEntitiesEventListener(EntityEventQueue<TEntityEvent>.EntityEventListener callback)
		{
			AllEntityEventListeners.Remove(callback);
		}

		public void RemoveEntityEventListener(EntId targetEntId, EntityEventQueue<TEntityEvent>.EntityEventListener callback)
		{
			if (!EntityEventListeners.TryGetValue(targetEntId, out List<EntityEventQueue<TEntityEvent>.EntityEventListener> eventListeners))
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
													out List<EntityEventQueue<TEntityEvent>.EntityEventListener> eventListeners))
				{
					foreach (EntityEventQueue<TEntityEvent>.EntityEventListener eventListener in eventListeners)
						eventListener.Invoke(entityEvent);
				}

				foreach (EntityEventQueue<TEntityEvent>.EntityEventListener eventListener in AllEntityEventListeners)
					eventListener.Invoke(entityEvent);
				
				entityEvent.Dispose();
			}
			
		}
	}

	public sealed class EntityEventQueuesContainer
	{
		private Dictionary<Type, BaseEntityEventQueueImplementation> _entityEventQueuesByEventType = new();

		internal EntityEventQueuesContainer()
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
			
			// return _entityEventQueuesByEventType;
			// foreach ( in _entityEventQueuesByEventType)
			// {
			// }
		}

		internal void ClearAll()
		{
			_entityEventQueuesByEventType.Clear();
		}

		internal void RemoveEntity(EntId entId)
		{
			foreach (BaseEntityEventQueueImplementation entityEventQueue in _entityEventQueuesByEventType.Values)
				entityEventQueue.RemoveEntity(entId);
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
	}
	
	

}