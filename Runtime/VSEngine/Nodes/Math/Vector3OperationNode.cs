using Core.Utils;
using FixedPointy;
using UnityEngine;

namespace Core.VSEngine.Nodes.Math
{
	[CreateNodeMenu(VSNodeMenuNames.MATH_MENU+"/[Vector3] x [Vector3] operations", order = VSNodeMenuNames.IMPORTANT)]
	[NodeTint(VSNodeMenuNames.MATH_NODES_TINT)]
	public class Vector3OperationNode: ValueOnlyNode
	{
		
		[SerializeField]
		private Vector3MathOperation type = Vector3MathOperation.Sum;
        
		[Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict), SerializeField]
		private Vector3 InputA = Vector3.zero;
		
		[Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict), SerializeField]
		private Vector3 InputB = Vector3.zero;

		[Output(ShowBackingValue.Never, ConnectionType.Multiple, TypeConstraint.Strict), SerializeField]
		private Vector3 Result;
		
		public enum Vector3MathOperation
		{
			Sum, 
			Subtraction,
		}

		
		public override OperationResult<object> GetValue(string portName)
		{
			OperationResult<Vector3> operationResult = Resolve<Vector3>(nameof(InputA));
			if (operationResult.IsFailure)
				return INVALID_INPUT<object>($"Failed to resolve InputA for Vector3OperationNode {name} in graph {graph.name}: {operationResult.Exception}");

			OperationResult<Vector3> operationResultB = Resolve<Vector3>(nameof(InputB));
			if (operationResultB.IsFailure)
				return INVALID_INPUT<object>($"Failed to resolve InputB for Vector3OperationNode {name} in graph {graph.name}: {operationResultB.Exception}");
			
			switch (type)
			{
				case Vector3MathOperation.Sum:
					Result = operationResult.Result + operationResultB.Result;
					break;
				case Vector3MathOperation.Subtraction:
					Result = operationResult.Result - operationResultB.Result;
					break;
			}
			return OperationResult<object>.Success(Result);
		}
	}
}