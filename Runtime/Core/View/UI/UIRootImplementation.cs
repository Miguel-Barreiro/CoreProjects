using System;
using System.Collections.Generic;
using System.Reflection;
using Core.Initialization;
using Core.Utils;
using Core.Utils.Reflection;
using UnityEngine;
using Zenject;

namespace Core.View.UI
{
	public sealed class UIRootImplementation : UIRoot
	{
		public const string ROOT_CANVAS_NAME = "RootCanvas";
		
		[Inject] private ObjectBuilder objectBuilder = null!;

		private readonly Canvas UICanvasRoot;
		private readonly Canvas UICanvasRootPrefab;
		
		private ListStack<ActiveView> _activeViews = new();
		private Dictionary<UIScreenDefinition, ActiveView> _viewsByDefinition = new();

		private Dictionary<UIScreenDefinition, UIMessenger> _messengersByScreenDefinitions = new();

#region Public


		public UIRootImplementation(Canvas uiCanvasRoot, Canvas rootUIPrefab)
		{
			this.UICanvasRoot = uiCanvasRoot; 
			this.UICanvasRootPrefab = rootUIPrefab;
		}


		public void Register(UIScreenDefinition uiFDefinition, UIMessenger uiMessenger)
		{
			if (_messengersByScreenDefinitions.ContainsKey(uiFDefinition))
				UpdateMessenger(uiFDefinition, uiMessenger);
			else
				_messengersByScreenDefinitions.Add(uiFDefinition, uiMessenger);
		}

		private void UpdateMessenger(UIScreenDefinition uiFDefinition, UIMessenger uiMessenger)
		{
			bool existsUI = _viewsByDefinition.TryGetValue(uiFDefinition, out ActiveView activeViewResult);
			if (existsUI)
			{
				ARGUMENT[0] = _messengersByScreenDefinitions[uiFDefinition];
				BaseUIView baseUIView = activeViewResult.ViewTransform.gameObject.GetComponent<BaseUIView>();
				activeViewResult.CachedUnRegisterMethod.Invoke(baseUIView, ARGUMENT);
			}
			
			_messengersByScreenDefinitions[uiFDefinition] = uiMessenger;
			if (existsUI)
			{
				ARGUMENT[0] = uiMessenger;
				BaseUIView baseUIView = activeViewResult.ViewTransform.gameObject.GetComponent<BaseUIView>();
				activeViewResult.CachedRegisterMethod.Invoke(baseUIView, ARGUMENT);
			}

		}

		public void RegisterWithViewObject<TMessenger>(UIScreenDefinition uiFDefinition,
														TMessenger uiMessenger, UIView<TMessenger> view)
			where TMessenger : UIMessenger
		{
			if (!_viewsByDefinition.TryGetValue(uiFDefinition, out ActiveView activeViewResult))
			{
				GameObject uiPrefab = uiFDefinition.UIPrefab;
				
				GameObject newCanvasObj = CreateUIViewRootCanvas(uiPrefab, out Canvas canvas);

				GameObject viewObject = view.gameObject;
				RectTransform rectTransform = viewObject.GetComponent<RectTransform>();

				rectTransform.parent = newCanvasObj.transform;
				_messengersByScreenDefinitions[uiFDefinition] = uiMessenger;
				
				activeViewResult = new ActiveView(rectTransform, canvas , uiFDefinition, uiMessenger);

				_viewsByDefinition.Add(uiFDefinition, activeViewResult);
				
				ARGUMENT[0] = uiMessenger;
				BaseUIView baseUIView = activeViewResult.ViewTransform.gameObject.GetComponent<BaseUIView>();
				activeViewResult.CachedRegisterMethod.Invoke(baseUIView, ARGUMENT);
			}

			Show(uiFDefinition);
		}



		public void Show(UIScreenDefinition uiFDefinition)
		{
			ActiveView activeView = MakeActiveView(uiFDefinition);
			activeView.Canvas.gameObject.SetActive(true);
			_activeViews.Remove(activeView);
			_activeViews.Add(activeView);
			activeView.Canvas.sortingOrder = _activeViews.Count;
			
			
			_messengersByScreenDefinitions[uiFDefinition]?.SignalShow();
		}


		public void Hide(UIScreenDefinition uiFDefinition)
		{
			ActiveView activeView;
			if (_viewsByDefinition.TryGetValue(uiFDefinition, out activeView))
			{
				activeView.Canvas.gameObject.SetActive(false);
				activeView.Canvas.sortingOrder = 0;
			}

			_activeViews.Remove(activeView);

			ActiveView newTopView = _activeViews.Top();
			if (newTopView != null)
			{
				newTopView.Canvas.gameObject.SetActive(true);
				newTopView.Canvas.sortingOrder = _activeViews.Count;
			}
			
			_messengersByScreenDefinitions[uiFDefinition]?.SignalHide();
		}

		public void HideAll()
		{
			foreach (ActiveView activeView in _activeViews)
			{
				activeView.Canvas.gameObject.SetActive(false);
				activeView.Canvas.sortingOrder = 0;
			}
			_activeViews.Clear();
			foreach ((UIScreenDefinition _,UIMessenger uiMessenger)  in _messengersByScreenDefinitions)
				uiMessenger.SignalHide();
		}
		
#endregion

#region Internal


		private readonly static object[] ARGUMENT = new[] {(object) null};
		private ActiveView MakeActiveView(UIScreenDefinition uiFDefinition)
		{

			ActiveView activeViewResult;
			
			if (!_viewsByDefinition.TryGetValue(uiFDefinition, out activeViewResult))
			{
				GameObject uiPrefab = uiFDefinition.UIPrefab;
				
				GameObject newCanvasObj = CreateUIViewRootCanvas(uiPrefab, out Canvas canvas);
			
				GameObject viewObject = objectBuilder.Instantiate(uiPrefab, canvas.transform);
				RectTransform rectTransform = viewObject.GetComponent<RectTransform>();

				UIMessenger uiMessenger = _messengersByScreenDefinitions[uiFDefinition];
				activeViewResult = new ActiveView(rectTransform, canvas , uiFDefinition, uiMessenger);

				_viewsByDefinition.Add(uiFDefinition, activeViewResult);
				
				ARGUMENT[0] = uiMessenger;
				BaseUIView baseUIView = activeViewResult.ViewTransform.gameObject.GetComponent<BaseUIView>();
				activeViewResult.CachedRegisterMethod.Invoke(baseUIView, ARGUMENT);
			}
			
			return activeViewResult;
		}

		private GameObject CreateUIViewRootCanvas(GameObject uiPrefab, out Canvas canvas)
		{
			GameObject newCanvasObj = GameObject.Instantiate(UICanvasRootPrefab.gameObject, UICanvasRoot.transform);
			newCanvasObj.name = uiPrefab.gameObject.name;
			newCanvasObj.SetActive(false);
			// newCanvasObj.transform.parent = UICanvasRoot.transform;
			canvas = newCanvasObj.GetComponent<Canvas>();
			canvas.overrideSorting = true;
			canvas.sortingOrder = 0;

			RectTransform rectTransform = canvas.GetComponent<RectTransform>();

			rectTransform.anchorMax = Vector2.one;
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.localScale = Vector3.one;
			
			return newCanvasObj;
		}

		

		private record ActiveView
		{
			public readonly RectTransform ViewTransform;
			public readonly Canvas Canvas;
			public readonly UIScreenDefinition Definition;
			public readonly MethodInfo CachedRegisterMethod;
			public readonly MethodInfo CachedUnRegisterMethod;

			
			public ActiveView(RectTransform viewTransform, Canvas canvas, UIScreenDefinition definition, UIMessenger messenger)
			{
				ViewTransform = viewTransform;
				Canvas = canvas;
				Definition = definition;
				BaseUIView baseUIView = viewTransform.gameObject.GetComponent<BaseUIView>();
				Type viewType = baseUIView.GetType();
				
				CachedRegisterMethod = viewType.GetMethodExt(nameof(UIView<UIMessenger>.Register),
															BindingFlags.Public, messenger.GetType());
				
				
				CachedUnRegisterMethod = viewType.GetMethodExt(nameof(UIView<UIMessenger>.Unregister),
																BindingFlags.Public, messenger.GetType());

			}
		}

#endregion

	}

}
