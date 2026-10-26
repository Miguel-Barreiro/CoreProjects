using Core.Model;
using UnityEngine;

namespace Core.View
{
	public abstract class EntityView  : MonoBehaviour, IViewController
	{
		public EntId EntityID { get; set; } = EntId.Invalid;
		public virtual void Reset() { }
		public abstract bool IsActive { get; }
		public abstract void ActivateGameObject();
		public abstract void DeactivateGameObject();
	}
}
