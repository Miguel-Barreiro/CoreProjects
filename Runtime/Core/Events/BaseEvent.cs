using System;
using System.Collections.Generic;
using Core.Systems;
using Core.Zenject.Source.Factories.Pooling.Static;
using Zenject;

namespace Core.Events
{
	public abstract class BaseEvent : IDisposable
	{
		public abstract void Execute();
		public abstract void Dispose();
		
		// public EventOrder Order { get; }
		// protected BaseEvent(EventOrder eventOrder)
		// {
		// 	this.Order = eventOrder;
		// }

		protected BaseEvent() { }

		public abstract void CallPreListenerSystemsInternal();
		public abstract void CallPostListenerSystemsInternal();
	}

	public abstract class Event<TEvent> : BaseEvent, IDisposable 
		where TEvent : Event<TEvent>, new() 
	{

		[Inject] private readonly SystemsContainer SystemsContainer = null!; 
		
		public static readonly StaticMemoryPool<TEvent> Pool =
			new StaticMemoryPool<TEvent>(OnSpawned, OnDespawned);

		protected virtual void OnSpawned() { }
		protected virtual void OnDespawned() { }

		private static void OnDespawned(TEvent obj) {
			(obj as Event<TEvent>)?.OnDespawned();
		}

		private static void OnSpawned(TEvent obj)
		{
			(obj as Event<TEvent>)?.OnSpawned();
		}

		public override void Dispose()
		{
			Pool.Despawn(this as TEvent);
		}

		public override void CallPreListenerSystemsInternal()
		{
			IEnumerable<EventListenerSystemsContainer.EventListenerSystemCache> listenerSystems;
			listenerSystems = SystemsContainer.GetAllEventListenerSystems<TEvent>();
			
			foreach (EventListenerSystemsContainer.EventListenerSystemCache listenerSystem in listenerSystems)
			{
				listenerSystem.CallOnEvent(this);
			}
		}
		
		public override void CallPostListenerSystemsInternal()
		{
			IEnumerable<EventListenerSystemsContainer.EventListenerSystemCache> listenerPostEventSystems;
			listenerPostEventSystems = SystemsContainer.GetAllPostEventListenerSystems<TEvent>();
			
			foreach (EventListenerSystemsContainer.EventListenerSystemCache listenerSystem in listenerPostEventSystems)
			{
				listenerSystem.CallOnEvent(this);
			}
		}

		// protected Event(EventOrder eventOrder = EventOrder.Default) : base(eventOrder) { }
		
	}

}