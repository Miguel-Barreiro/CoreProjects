using UnityEngine;

namespace Core.View.UI
{
	public interface UIRoot
	{
		public void Show(UIScreenDefinition screen);

		public void Hide(UIScreenDefinition screen);

		public void HideAll();

		public Canvas UICanvasRoot { get; }

	}
	

}