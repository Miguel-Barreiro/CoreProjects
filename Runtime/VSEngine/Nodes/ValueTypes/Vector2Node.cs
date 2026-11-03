using Core.Utils;
using UnityEngine;

namespace Core.VSEngine.Nodes
{
	[CreateNodeMenu(VSNodeMenuNames.VALUES_MENU +"/Vector2", order = VSNodeMenuNames.IMPORTANT)]
	[NodeWidth(250)]
	public sealed class Vector2Node : SimpleValueNode<Vector2>
	{
		[SerializeField] private Vector2 VectorValue;

		public override OperationResult<object> GetValue(string portName)
		{
			return SUCCESS_RETURN<object>(VectorValue);
		}
	}
}