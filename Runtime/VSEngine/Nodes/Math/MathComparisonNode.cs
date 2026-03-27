using System;
using Core.Utils;
using FixedPointy;
using UnityEditor;
using UnityEngine;
using XNode;
using MathUtils = Core.Utils.MathUtils;

namespace Core.VSEngine.Nodes.Math
{
	[Node.CreateNodeMenu(VSNodeMenuNames.MATH_MENU+"/Compare", order = VSNodeMenuNames.IMPORTANT)]
	[Node.NodeTint(VSNodeMenuNames.CONDITION_NODES_TINT)]
	[Serializable]
	public class MathComparisonNode : ValueOnlyNode
	{
		[Input(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Override, 
				backingValue = ShowBackingValue.Never), SerializeField]
		private Fix a;

		// [Input(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Override,
		// 		backingValue = ShowBackingValue.Always), SerializeField]
		[SerializeField]
		private MathComparisons comparison;

		[Input(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Override, 
				backingValue = ShowBackingValue.Never), SerializeField]
		private Fix b;

		[Output(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Multiple), SerializeField]
		private bool Result;
		
		public override OperationResult<object> GetValue(string portName)
		{
			OperationResult<object> CompareAToB()
			{
				OperationResult<Fix> operationResultA = Resolve<Fix>(nameof(a));
				if (operationResultA.IsFailure)
					return INVALID_INPUT<object>(nameof(a));

				OperationResult<Fix> operationResultB = Resolve<Fix>(nameof(b));
				if (operationResultB.IsFailure)
					return INVALID_INPUT<object>(nameof(b));

				OperationResult<MathComparisons> operationResultComp = Resolve<MathComparisons>(nameof(comparison));
				if (operationResultComp.IsFailure)
					return INVALID_INPUT<object>(nameof(comparison));
				
				
				Fix aValue = operationResultA.Result;
				Fix bValue = operationResultB.Result;
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