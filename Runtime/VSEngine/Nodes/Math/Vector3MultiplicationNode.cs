using Core.Utils;
using FixedPointy;
using UnityEngine;

namespace Core.VSEngine.Nodes.Math
{

	[CreateNodeMenu(VSNodeMenuNames.MATH_MENU+"/[Vector3] x [Number]", order = VSNodeMenuNames.IMPORTANT)]
	[NodeTint(VSNodeMenuNames.MATH_NODES_TINT)]
	public sealed class Vector3MultiplicationNode: ValueOnlyNode
	{
		
		[Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict), SerializeField]
		private Vector3 InputA = Vector3.zero;
		
		[Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict), SerializeField]
		private Fix InputB = Fix.One;

		[Output(ShowBackingValue.Never, ConnectionType.Multiple, TypeConstraint.Strict), SerializeField]
		private Vector3 Result;
		
		public override OperationResult<object> GetValue(string portName)
		{
			OperationResult<Vector3> operationResult = Resolve<Vector3>(nameof(InputA));
			if (operationResult.IsFailure)
				return INVALID_INPUT<object>($"Failed to resolve InputA for Vector3MultiplicationNode {name} in graph {graph.name}: {operationResult.Exception}");

			OperationResult<Fix> operationResultB = Resolve<Fix>(nameof(InputB));
			if (operationResultB.IsFailure)
				return INVALID_INPUT<object>($"Failed to resolve InputB for Vector3MultiplicationNode {name} in graph {graph.name}: {operationResultB.Exception}");

			Vector3 vector3 = operationResult.Result;
			FixVec3 fixVec3 = new FixVec3(vector3.x, vector3.y, vector3.z);
			return OperationResult<object>.Success(fixVec3 * operationResultB.Result);
		}
	}

}
