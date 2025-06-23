using Core.Model;
using UnityEngine;

namespace Core.View
{
	public abstract class EntityView  : MonoBehaviour
	{
		public EntId EntityID { get; set; } = EntId.Invalid;
		public virtual void Reset() { }
	}
}