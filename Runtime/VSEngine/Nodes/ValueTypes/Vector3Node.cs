using Core.Utils;
using UnityEngine;

namespace Core.VSEngine.Nodes
{
	[CreateNodeMenu(VSNodeMenuNames.VALUES_MENU +"/Vector3", order = VSNodeMenuNames.IMPORTANT)]
	[NodeWidth(300)]
	public sealed class Vector3Node : SimpleValueNode<Vector3>
	{
		[SerializeField] private Vector3 VectorValue;

		public override OperationResult<object> GetValue(string portName)
		{
			return SUCCESS_RETURN<object>(VectorValue);
		}
	}
}