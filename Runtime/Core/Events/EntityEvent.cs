using System;
using Core.Model;
using Core.Zenject.Source.Factories.Pooling.Static;
using UnityEngine;

namespace Core.Events
{
	public abstract class BaseEntityEvent : IBaseEvent, IDisposable
	{
		public EntId EntityID { get; private set; } = EntId.Invalid;
		
		internal void SetTargetEntityID(EntId targetEntityID)
		{
			this.EntityID = targetEntityID;
		}
		
		public abstract void Execute();
		public abstract void Dispose();
		

		private bool isPropagating = true;
		public bool IsPropagating => isPropagating;
        
        
		public void StopPropagation()
		{
			Debug.Log($"VS: Stopped propagation on event {this.GetType().Name}");
			isPropagating = false;
		}
		
		public abstract void CallPreListenerSystemsInternal();
		public abstract void CallPostListenerSystemsInternal();

	}
	
	
	public abstract class EntityEvent<TEvent> : BaseEntityEvent, IDisposable 
		where TEvent : EntityEvent<TEvent>, new() 
	{
		
		internal static readonly StaticMemoryPool<TEvent> Pool = new StaticMemoryPool<TEvent>(OnSpawned, OnDespawned);

		protected virtual void OnSpawned() { }
		protected virtual void OnDespawned() { }

		private static void OnDespawned(TEvent obj) {
			obj.OnDespawned();
		}

		private static void OnSpawned(TEvent obj)
		{
			obj.OnSpawned();
		}

		public override void Dispose()
		{
			Pool.Despawn(this as TEvent);
		}

		public override void CallPreListenerSystemsInternal() { }
		public override void CallPostListenerSystemsInternal() { }
	}
}