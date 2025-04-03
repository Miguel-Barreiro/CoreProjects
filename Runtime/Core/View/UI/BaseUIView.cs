#nullable enable

using UnityEngine;

namespace Core.View.UI
{
	
	public class BaseUIView : MonoBehaviour
	{
		
	}

	public abstract class UIView<TUIMessenger> : BaseUIView
		where TUIMessenger : UIMessenger
	{

		protected TUIMessenger Messenger = null!;
		
		public void Register(TUIMessenger uiMessenger)
		{
			Messenger = uiMessenger;
			uiMessenger.OnShow += OnShow;
			uiMessenger.OnHide += OnHide;
			OnRegister(uiMessenger);
		}
		public void Unregister(TUIMessenger uiMessenger)
		{
			Messenger = null!;
			uiMessenger.OnShow -= OnShow;
			uiMessenger.OnHide -= OnHide;
			OnUnregister(uiMessenger);
		}

		protected virtual void OnShow() { }
		protected virtual void OnHide() { }

		
		protected abstract void OnUnregister(TUIMessenger uiMessenger);

		protected abstract void OnRegister(TUIMessenger uiMessenger);

	}
	
}