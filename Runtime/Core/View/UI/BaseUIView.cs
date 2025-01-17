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
		public abstract void Register(TUIMessenger uiEvent);
	}
	
}