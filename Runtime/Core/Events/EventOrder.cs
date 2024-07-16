namespace Core.Events
{
	public enum EventOrder
	{
		PreDefault = -10, 
		Default = 0,
		PostDefault = 10,
	}


	public interface IEarlyEvent
	{
		public EventOrder Order { get => EventOrder.PreDefault; }
	}
	
	public interface ILateEvent
	{
		public EventOrder Order { get => EventOrder.PostDefault; }
	}
	
	
}
