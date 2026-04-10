
using Core.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.VSEngine.Nodes
{
	[CreateNodeMenu(VSNodeMenuNames.VALUES_MENU +"/Bool", order = VSNodeMenuNames.IMPORTANT)]
	[NodeTint(VSNodeMenuNames.VALUES_NODES_TINT)]
	public class BoolNode : SimpleValueNode<bool>
	{
		[SerializeField, HideLabel]
		private bool BoolValue = true;
		
		public override OperationResult<object> GetValue(string portName)
		{
			return OperationResult<object>.Success(BoolValue);
		}
	}
}