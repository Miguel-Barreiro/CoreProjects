namespace Core.Events
{
	public interface IEventListener<TEvent>
		where TEvent : Event<TEvent>, new()
	{
		public void OnEvent(TEvent onEvent);
	}
}