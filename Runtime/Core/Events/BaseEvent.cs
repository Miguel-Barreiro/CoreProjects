using System;
using Core.Zenject.Source.Factories.Pooling.Static;

namespace Core.Events
{
	public abstract class BaseEvent : IDisposable
	{
		public abstract void Execute();
		public abstract void Dispose();
		
		public EventOrder Order { get; }
		protected BaseEvent(EventOrder eventOrder)
		{
			this.Order = eventOrder;
		}

	}

	public abstract class Event<TEvent> : BaseEvent, IDisposable where TEvent : class, new() 
	{
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

		protected Event(EventOrder eventOrder = EventOrder.Default) : base(eventOrder) { }

		// public static TEvent New()
		// {
		// 	TEvent newEvent = Pool.Spawn();
		// 	EventManager.Get().Execute(newEvent);
		// 	return newEvent;
		// }
	}

	public enum EventOrder
	{
		PreDefault = -10, 
		Default = 0,
		PostDefault = 10,
	}
}