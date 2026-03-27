using System;
using Core.Utils;
using FixedPointy;
using UnityEngine;
using XNode;

namespace Core.VSEngine.Nodes.Math
{
	[Node.CreateNodeMenu(VSNodeMenuNames.VALUES_MENU +"/Convert", order = VSNodeMenuNames.IMPORTANT)]
	[Node.NodeTint(VSNodeMenuNames.NOT_WORKING_TINT)]
	[Serializable]
	public sealed class ConvertTypesNode : ValueOnlyNode
	{
		[Header("NOT WORKING")]
		[Input(typeConstraint = TypeConstraint.Inherited, 
				connectionType = ConnectionType.Override, 
				backingValue = ShowBackingValue.Never), SerializeField]
		private Fix Input;
		
		public override OperationResult<object> GetValue(string portName)
		{
			throw new System.NotImplementedException();
		}
	}
}