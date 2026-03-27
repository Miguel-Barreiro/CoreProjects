using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.VSEngine.Nodes
{
	public abstract class SimpleValueNode<T> : ValueOnlyNode
	{
		[Output(ShowBackingValue.Never), SerializeField, HideLabel]
		private T Value;

	}
}