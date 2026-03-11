using System;
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
	}
}