using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.VSEngine.Nodes
{
	[NodeTint(VSNodeMenuNames.VALUES_NODES_TINT)]
	public abstract class SimpleValueNode<T> : ValueOnlyNode
	{
		[Output(ShowBackingValue.Never), SerializeField, HideLabel]
		private T Value;

	}
}