namespace Core.Events
{
	public interface IEventListener<TEvent>
		where TEvent : Event<TEvent>, new()
	{
		public void OnEvent(TEvent onEvent);
	}

	public interface IPostEventListener<TEvent>
		where TEvent : Event<TEvent>, new()
	{
		public void OnPostEvent(TEvent onEvent);
	}

	
}