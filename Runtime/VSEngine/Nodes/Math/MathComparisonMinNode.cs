using System;
using Core.Utils;
using FixedPointy;
using UnityEngine;
using XNode;

namespace Core.VSEngine.Nodes.Math
{
	[Node.CreateNodeMenu(VSNodeMenuNames.MATH_MENU+"/Compare with const", order = VSNodeMenuNames.IMPORTANT)]
	[Node.NodeTint(VSNodeMenuNames.CONDITION_NODES_TINT)]
	[Serializable]
	public sealed class MathComparisonMinNode : ValueOnlyNode
	{
		[Input(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Override, 
				backingValue = ShowBackingValue.Never), SerializeField]
		private Fix a;

		// [Input(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Override,
		// 		backingValue = ShowBackingValue.Always), SerializeField]
		[SerializeField]
		private MathComparisons comparison;

		[SerializeField]
		private float b;

		[Output(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Multiple), SerializeField]
		private bool Result;
		
		public override OperationResult<object> GetValue(string portName)
		{
			OperationResult<object> CompareAToB()
			{
				OperationResult<Fix> operationResultA = Resolve<Fix>(nameof(a));
				if (operationResultA.IsFailure)
					return INVALID_INPUT<object>(nameof(a));
				
				OperationResult<MathComparisons> operationResultComp = Resolve<MathComparisons>(nameof(comparison));
				if (operationResultComp.IsFailure)
					return INVALID_INPUT<object>(nameof(comparison));
				
				
				Fix aValue = operationResultA.Result;
				Fix bValue = b;
				return SUCCESS_RETURN<object>(MathUtils.MakeComparison(aValue, bValue, operationResultComp.Result));
			}

			return portName switch
			{
				nameof(Result) => CompareAToB(),
				_ => INVALID_INPUT_NAME_CALLED<object>(portName),
			};
			
		}
	}

}