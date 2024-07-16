using System;
using System.Collections.Generic;
using Core.Initialization;
using Core.Model;
using Core.Systems;
using Core.Utils;
using Core.Utils.CachedDataStructures;
using Zenject;

#nullable enable

namespace Core.Events
{
	public sealed class EventQueue: IInitSystem
	{
		[Inject] private readonly ObjectBuilder ObjectBuilder = null!;
		
		private readonly List<EventOrder> ProcessOrder = new ();
		private readonly Dictionary<EventOrder, Dictionary<Type, List<BaseEvent>>> EventQueueByType = new();
		
		public TEvent Execute<TEvent>() where TEvent : Event<TEvent>, new()
		{
			Type eventType = typeof(TEvent);
			
			TEvent newEvent = Event<TEvent>.Pool.Spawn();
			ObjectBuilder.Inject(newEvent);

			TypeCache.EventAttributes attributes = TypeCache.Get().GetEventAttributes(eventType);

			EventOrder newEventOrder = attributes.EventOrder;
			this.EventQueueByType[newEventOrder][eventType].Add(newEvent);
			return newEvent;
		}
		
		public int EventsCount => GetEventsCount();
		private int GetEventsCount()
		{
			int eventCount = 0;
			foreach (EventOrder eventOrder in ProcessOrder)
			{
				eventCount += EventQueueByType[eventOrder].Count;
			}

			return eventCount;
		}

		internal IEnumerable<BaseEvent> PopEvents()
		{
			using CachedList<BaseEvent> doneEvents = ListCache<BaseEvent>.Get();
			using CachedList<BaseEvent> currentEventList = ListCache<BaseEvent>.Get();
			foreach (EventOrder eventOrder in ProcessOrder)
			{
				Dictionary<Type,List<BaseEvent>> eventsByOrder = EventQueueByType[eventOrder];
				foreach ((Type eventType, List<BaseEvent> currentQueu) in eventsByOrder)
				{
					currentEventList.Clear();
					currentEventList.AddRange(currentQueu);
					currentQueu.Clear();
					foreach (BaseEvent currentEvent in currentEventList)
					{
						yield return currentEvent;
						doneEvents.Add(currentEvent);
					}
				}
			}

			foreach (BaseEvent doneEvent in doneEvents)
			{
				doneEvent.Dispose();
			}
		}
		
		public EventQueue()
		{
			foreach (EventOrder eventOrder in EnumUtils.GetValues<EventOrder>())
			{
				EventQueueByType.Add(eventOrder, new Dictionary<Type, List<BaseEvent>>());
				ProcessOrder.Add(eventOrder);
			}

			ProcessOrder.Sort();
		}

		public void Initialize()
		{
			IEnumerable<TypeCache.EventAttributes> allEventTypes = TypeCache.Get().GetAllEventAttributes();
			foreach (TypeCache.EventAttributes eventAttributes in allEventTypes)
			{
				EventQueueByType[eventAttributes.EventOrder].Add(eventAttributes.EventType, new List<BaseEvent>());
			}
		}
	}


	// public interface ITest<T>
	// {
	// 	public void OnTest(T arg);
	// }
	//
	//
	// public class Event1 { }
	// public class Event2 { }
	//
	// public class Test1 : ITest<Event1>, ITest<Event2>
	// {
	// 	public void OnTest(Event1 arg) { throw new System.NotImplementedException(); }
	// 	public void OnTest(Event2 arg) { throw new System.NotImplementedException(); }
	// }
}