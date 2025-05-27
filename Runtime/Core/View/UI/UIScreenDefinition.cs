using Core.Model.Data;
using UnityEngine;

namespace Core.View.UI
{
	[CreateAssetMenu(fileName = "UIScreenDefinition", menuName = "!Game/UI/ScreenDefinition", order = 100)]
	public class UIScreenDefinition : DataConfig
	{
		[SerializeField] private UILifetimeType lifetimeType;
		public UILifetimeType LifetimeType => lifetimeType;
		
		[SerializeField] private GameObject uiPrefab = null;
		public GameObject UIPrefab => uiPrefab;
	}


	public enum UILifetimeType
	{
		Cached, 
		NewInstance
	}

}