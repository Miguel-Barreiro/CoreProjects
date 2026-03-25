
using System;
using Core.Events;

namespace Core.VSEngine.Nodes.Events
{
	[Serializable]
	public sealed class EventSerializedType : SerializedType
	{
		public EventSerializedType(Type type) : base(type) { }
		public EventSerializedType() : base(typeof(Event<>)) { }
	}
}