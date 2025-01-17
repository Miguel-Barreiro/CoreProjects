namespace Core.View.UI
{
	public interface UIController<TUIMessenger>
		where TUIMessenger : UIMessenger
	{
		public void Register(TUIMessenger uiMessenger);
	}
}