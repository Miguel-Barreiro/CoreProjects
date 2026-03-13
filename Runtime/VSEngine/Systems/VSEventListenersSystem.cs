using System;
using Core.Events;
using Core.Model;

namespace Core.VSEngine.Systems
{
	public interface IVSEventListenersSystem
	{
		public void AddListener(EntId owner, Type eventType, BaseEventListenNode node);
		public void RemoveListener(EntId owner, Type eventType, BaseEventListenNode node);
	}

	public sealed class VSEventListenersSystem : IVSEventListenersSystem
	{
		public void AddListener(EntId owner, Type eventType, BaseEventListenNode node)
		{
			
			throw new NotImplementedException();
		}

		public void RemoveListener(EntId owner, Type eventType, BaseEventListenNode node)
		{
			throw new NotImplementedException();
		}

		public void ExecutePreEntityEvent<TEntityEvent>(TEntityEvent entityEvent) where TEntityEvent : EntityEvent<TEntityEvent>, new()
		{
			
		}

		public void ExecuteEntityEvent<TEntityEvent>(TEntityEvent entityEvent) where TEntityEvent : EntityEvent<TEntityEvent>, new()
		{
			throw new NotImplementedException();
		}

		public void ExecutePostEntityEvent<TEntityEvent>(TEntityEvent entityEvent) where TEntityEvent : EntityEvent<TEntityEvent>, new()
		{
			throw new NotImplementedException();
		}

		public void ExecutePreEvent(BaseEvent currentEvent)
		{
			throw new NotImplementedException();
		}

		public void ExecuteEvent(BaseEvent currentEvent)
		{
			throw new NotImplementedException();
		}

		public void ExecutePostEvent(BaseEvent currentEvent)
		{
			throw new NotImplementedException();
		}
	}
}