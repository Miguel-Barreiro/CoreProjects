#nullable enable

using System;
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
			OnRegister(uiMessenger);
		}
		public void Unregister(TUIMessenger uiMessenger)
		{
			Messenger = null!;
			OnUnregister(uiMessenger);
		}


		protected abstract void OnUnregister(TUIMessenger uiMessenger);

		protected abstract void OnRegister(TUIMessenger uiMessenger);

	}
	
}