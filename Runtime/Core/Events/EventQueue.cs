using System.Collections.Generic;
using Core.Initialization;
using Core.Utils;
using Core.Utils.CachedDataStructures;
using Zenject;

#nullable enable

namespace Core.Events
{
	public sealed class EventQueue
	{
		[Inject] private readonly ObjectBuilder ObjectBuilder = null!;
		
		private readonly List<EventOrder> processOrder = new ();
		private readonly Dictionary<EventOrder, List<BaseEvent>> eventQueue = new();
		
		public TEvent Execute<TEvent>() where TEvent : Event<TEvent>, new()
		{
			TEvent newEvent = Event<TEvent>.Pool.Spawn();
			ObjectBuilder.Inject(newEvent);
			
			this.eventQueue[newEvent.Order].Add(newEvent);
			return newEvent;
		}

		public int EventsCount => GetEventsCount();
		public int GetEventsCount()
		{
			int eventCount = 0;
			foreach (EventOrder eventOrder in processOrder)
			{
				eventCount += eventQueue[eventOrder].Count;
			}

			return eventCount;
		}

		internal IEnumerable<BaseEvent> PopEvents()
		{
			using CachedList<BaseEvent> doneEvents = ListCache<BaseEvent>.Get();
			using CachedList<BaseEvent> currentEventList = ListCache<BaseEvent>.Get();
			foreach (EventOrder eventOrder in processOrder)
			{
				List<BaseEvent> currentQueu = eventQueue[eventOrder];
				currentEventList.Clear();
				currentEventList.AddRange(currentQueu);
				currentQueu.Clear();

				foreach (BaseEvent currentEvent in currentEventList)
				{
					yield return currentEvent;
					doneEvents.Add(currentEvent);
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
				eventQueue.Add(eventOrder, new List<BaseEvent>());
				processOrder.Add(eventOrder);
			}

			processOrder.Sort();
		}
		
		// private static EventQueue instance = null;
		// internal static EventQueue Get()
		// {
		// 	if (instance == null)
		// 	{
		// 		instance = new EventQueue();
		// 	}
		// 	return instance;
		// }

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