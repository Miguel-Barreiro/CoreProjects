using System;
using Core.Model;
using Core.Zenject.Source.Factories.Pooling.Static;

namespace Core.Events
{
	public abstract class BaseEntityEvent : IDisposable
	{
		public EntId EntityID { get; private set; } = EntId.Invalid;
		public abstract void Dispose();
		
		internal void SetTargetEntityID(EntId targetEntityID)
		{
			this.EntityID = targetEntityID;
		}
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
		
		
				
	}
}