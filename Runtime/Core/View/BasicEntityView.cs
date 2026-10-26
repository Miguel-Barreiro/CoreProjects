using Core.Model;
using UnityEngine;

namespace Core.View
{
	public class BasicEntityView : EntityView
	{
		public override bool IsActive => gameObject.activeSelf;

		public override void ActivateGameObject()
		{
			gameObject.SetActive(true);
		}

		public override void DeactivateGameObject()
		{
			gameObject.SetActive(false);
		}
	}
}