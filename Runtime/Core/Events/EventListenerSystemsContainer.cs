using System;
using System.Collections.Generic;
using System.Reflection;
using Core.Model;
using Core.Utils.CachedDataStructures;
using Core.Utils.Reflection;
using UnityEngine;
using Object = System.Object;

namespace Core.Events
{
	public sealed class EventListenerSystemsContainer
	{
		private readonly Dictionary<Type, List<EventListenerSystemCache>> systemsByListenerTypes = new();
		
		public IEnumerable<EventListenerSystemCache> GetAllEventListenerSystems<TEvent>()
			where TEvent : Event<TEvent>, new()
		{

			if (!systemsByListenerTypes.ContainsKey(typeof(TEvent)))
			{
				// Debug.Log($"no attributes exist for event {typeof(TEvent)}");
				yield break;
			}

			List<EventListenerSystemCache> systemsByListenerType = systemsByListenerTypes[typeof(TEvent)];
			foreach (EventListenerSystemCache system in systemsByListenerType)
			{
				yield return system;
			}
		}

		public void AddEventListener(object system)
		{
			Type type = system.GetType();
			IEnumerable<Type> allImplementedInterfaces = type.GetImplementedInterfaces();
			using CachedList<Type> implementedInterfaces = ListCache<Type>.Get(allImplementedInterfaces);

			IEnumerable<TypeCache.EventAttributes> allEventListenerTypes = TypeCache.Get().GetAllEventListenerTypes();
			using CachedList<TypeCache.EventAttributes> eventListenerTypes = ListCache<TypeCache.EventAttributes>.Get(allEventListenerTypes);
			
			foreach (Type implementedInterface in implementedInterfaces)
			{
				foreach (TypeCache.EventAttributes eventAttributes in eventListenerTypes)
				{
					if (implementedInterface == eventAttributes.EventListenerType)
					{
						// Debug.Log($"system {system.GetType()} has {implementedInterface.Name} == {eventAttributes.EventListenerType.Name}");
						EventListenerSystemCache newListenerSystemCache = new EventListenerSystemCache(system, eventAttributes.EventType);
						this.systemsByListenerTypes[eventAttributes.EventType].Add(newListenerSystemCache);
					}
				}
			}
		}

		public void RemoveEventListener(object system)
		{
			foreach ((Type _,List<EventListenerSystemCache> listenerSystems)  in systemsByListenerTypes)
			{
				listenerSystems.RemoveAll(cache => cache.System == system);
			}
		}

		public EventListenerSystemsContainer()
		{
			TypeCache typeCache = TypeCache.Get();
			foreach (Type eventType in typeCache.GetAllEventTypes())
			{
				// Debug.Log($"setting up event {eventType}"); 
				this.systemsByListenerTypes.Add(eventType, new List<EventListenerSystemCache>());
			}
		}
		
		
		public sealed class EventListenerSystemCache
		{
			public readonly Object System;
			private readonly MethodInfo CachedOnEventMethod;

			private readonly static object[] ARGUMENT = new[] {(object) null};
			
			public void CallOnEvent(BaseEvent eventTriggered)
			{
				ARGUMENT[0] = eventTriggered;
				CachedOnEventMethod.Invoke(System, ARGUMENT);
			}

			public EventListenerSystemCache(Object system, Type eventType)
			{
				System = system;
				Type systemType = system.GetType();

				// Type genericListeneriType = typeof(IEventListener<>);
				// Type eventListenerType = genericListeneriType.MakeGenericType( eventType );

				CachedOnEventMethod = systemType.GetMethodExt(nameof(IEventListener<OnProjectInstallCompleteEvent>.OnEvent),
															BindingFlags.Public, eventType);
			}
		}

		
		
	}
}