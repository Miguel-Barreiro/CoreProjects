using System;
using System.Collections.Generic;
using Core.Initialization;
using Core.Systems;
using Core.VSEngine.Systems;
using Core.Zenject.Source.Factories.Pooling.Static;
using UnityEngine;
using Zenject;

namespace Core.Events
{

	public interface IBaseEvent
	{
		public abstract void Execute();
		public abstract void Dispose();

		public bool IsPropagating { get; }
		public void StopPropagation();
		
		public abstract void CallPreListenerSystemsInternal();
		public abstract void CallPostListenerSystemsInternal();

	}

	public abstract class BaseEvent : IBaseEvent,  IDisposable
	{
		public abstract void Execute();
		public abstract void Dispose();


		private bool isPropagating = true;
		public bool IsPropagating => isPropagating;
        
        
		public void StopPropagation()
		{
			Debug.Log($"VS: Stopped propagation on event {this.GetType().Name}");
			isPropagating = false;
		}

		protected BaseEvent()
		{
			ObjectBuilder.GetInstance().Inject(this);
		}

		public abstract void CallPreListenerSystemsInternal();
		public abstract void CallPostListenerSystemsInternal();
	}

	public abstract class Event<TEvent> : BaseEvent, IDisposable 
		where TEvent : Event<TEvent>, new() 
	{
		
		[Inject] private readonly SystemsContainer SystemsContainer = null!; 
		
		internal static readonly StaticMemoryPool<TEvent> Pool =
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