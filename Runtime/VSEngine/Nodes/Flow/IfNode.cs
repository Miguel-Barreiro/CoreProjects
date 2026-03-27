using Core.Utils;
using UnityEngine;

namespace Core.VSEngine.Nodes
{
	[CreateNodeMenu(VSNodeMenuNames.FLOW_MENU+"/IF", order = VSNodeMenuNames.VERY_IMPORTANT)]
	public sealed class IfNode : BasicFlowNode
	{
		
		[Input(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Override, 
				backingValue = ShowBackingValue.Unconnected), SerializeField]
		private bool Condition;
		
		[Output(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Override), SerializeField]
		private Control? True;

		[Output(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Override, backingValue = ShowBackingValue.Never), SerializeField]
		private Control? False;


		protected override void Action()
		{
			OperationResult<bool> operationResult = Resolve<bool>(nameof(Condition));
			if(Check(operationResult.IsFailure, "Failed to resolve condition input"))
				return;
			
			if (operationResult.Result)
				ContinueWith(nameof(True));
			else
				ContinueWith(nameof(False));
		}

	}
}