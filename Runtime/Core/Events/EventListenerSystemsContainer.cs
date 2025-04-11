using System;
using System.Collections.Generic;
using System.Reflection;
using Core.Model;
using Core.Utils.CachedDataStructures;
using Core.Utils.Reflection;
using Object = System.Object;

namespace Core.Events
{
	public sealed class EventListenerSystemsContainer
	{
		private readonly Dictionary<Type, List<EventListenerSystemCache>> SystemsByListenerTypes = new();
		private readonly Dictionary<Type, List<EventListenerSystemCache>> SystemsByPostListenerTypes = new();
		
		public IEnumerable<EventListenerSystemCache> GetAllEventListenerSystems<TEvent>()
			where TEvent : Event<TEvent>, new()
		{

			if (!SystemsByListenerTypes.ContainsKey(typeof(TEvent)))
			{
				// Debug.Log($"no attributes exist for event {typeof(TEvent)}");
				yield break;
			}

			List<EventListenerSystemCache> systemsByListenerType = SystemsByListenerTypes[typeof(TEvent)];
			foreach (EventListenerSystemCache system in systemsByListenerType)
			{
				yield return system;
			}
		}
		
		public IEnumerable<EventListenerSystemCache> GetAllPostEventListenerSystems<TEvent>() where TEvent : Event<TEvent>, new()
		{
			if (!SystemsByPostListenerTypes.ContainsKey(typeof(TEvent)))
			{
				// Debug.Log($"no attributes exist for event {typeof(TEvent)}");
				yield break;
			}

			List<EventListenerSystemCache> systemsByListenerType = SystemsByPostListenerTypes[typeof(TEvent)];
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
					if (implementedInterface != eventAttributes.EventListenerType)
						continue;

					// Debug.Log($"system {system.GetType()} has {implementedInterface.Name} == {eventAttributes.EventListenerType.Name}");
					EventListenerSystemCache newListenerSystemCache = new EventListenerSystemCache(system, 
																									eventAttributes.EventType,
																									false);
					this.SystemsByListenerTypes[eventAttributes.EventType].Add(newListenerSystemCache);
				}
			}
		}

		public void AddPostEventListener(object system)
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
					if (implementedInterface != eventAttributes.PostEventListenerType)
						continue;

					// Debug.Log($"system {system.GetType()} has {implementedInterface.Name} == {eventAttributes.EventListenerType.Name}");
					EventListenerSystemCache newListenerSystemCache = new EventListenerSystemCache(system, 
																									eventAttributes.EventType,
																									true);
					SystemsByPostListenerTypes[eventAttributes.EventType].Add(newListenerSystemCache);
				}
			}
			
		}


		public void RemovePostEventListener(object system)
		{
			foreach ((Type _,List<EventListenerSystemCache> listenerSystems)  in SystemsByPostListenerTypes)
			{
				listenerSystems.RemoveAll(cache => cache.System == system);
			}
		}

		public void RemoveEventListener(object system)
		{
			foreach ((Type _,List<EventListenerSystemCache> listenerSystems)  in SystemsByListenerTypes)
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
				SystemsByListenerTypes.Add(eventType, new List<EventListenerSystemCache>());
				SystemsByPostListenerTypes.Add(eventType, new List<EventListenerSystemCache>());
			}
			
		}
		
		
		private const string ON_EVENT_NAME = nameof(IEventListener<OnProjectInstallCompleteEvent>.OnEvent);
		private const string ON_POST_EVENT_NAME = nameof(IPostEventListener<OnProjectInstallCompleteEvent>.OnPostEvent);
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

			public EventListenerSystemCache(Object system, Type eventType, bool isPost)
			{
				System = system;
				Type systemType = system.GetType();

				// Type genericListeneriType = typeof(IEventListener<>);
				// Type eventListenerType = genericListeneriType.MakeGenericType( eventType );

				if (isPost)
				{
					CachedOnEventMethod = systemType.GetMethodExt(ON_POST_EVENT_NAME, 
																BindingFlags.Public, eventType);
					
				} else
				{
					CachedOnEventMethod = systemType.GetMethodExt(ON_EVENT_NAME,
																BindingFlags.Public, eventType);

				}
			}
		}


	}
}