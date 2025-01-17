using System;

namespace Core.View.UI
{
	public abstract class UIMessenger
	{
		public event Action OnHide = null!;
		public event Action OnShow = null!;
		
		internal void SignalHide() { OnHide?.Invoke(); }
		internal void SignalShow() { OnShow?.Invoke(); }
		
	}
}