using Core.Utils;
using FixedPointy;
using NUnit.Framework;
using UnityEngine;
using XNode;

namespace Core.VSEngine.Nodes.TestNodes
{
	// [Node.CreateNodeMenu(VSNodeMenuNames.TEST_MENU+"/" + VSNodeMenuNames.UNIT_TEST_MENU +"/Assert", order = VSNodeMenuNames.LOW)]
	[Node.NodeTint(VSNodeMenuNames.DEBUG_NODES_TINT)]
	public abstract class BaseTestAssertNode : BasicFlowNode {

		[SerializeField]
		[TextArea(2, 10)]
		private string Header;

		// [Input(ShowBackingValue.Unconnected, ConnectionType.Override, TypeConstraint.Strict), SerializeField]
		// private Fix In;
		//
		// [Output(ShowBackingValue.Unconnected, ConnectionType.Override, TypeConstraint.Strict), SerializeField]
		// private Fix Out;

		protected override void Action()
		{
			Debug.Log($"{Header} in node {this.name} in {graph.name}");
			Assert();
		}

		protected abstract void Assert();

		protected void ASSERT_EQUAL<T>(T expected, T actual)
		{
			NUnit.Framework.Assert.That(expected, 
										expression:Is.EqualTo(actual)
										message: );
		}

		protected void ASSERT_EXIST<T>(OperationResult<T> input)
		{
			NUnit.Framework.Assert.That(input.IsFailure, Is.Not.True);
		}
		
		// protected override void Action()
		// {
		// 	OperationResult<Fix> operationResult = Resolve<Fix>(nameof(In));
		// 	if (operationResult.IsSuccess)
		// 	{
		// 		Debug.Log($"<TEST_ALL_NODE><{operationResult.Result}>");
		// 	}
		//
		// 	Debug.Log("<TEST_ALL_NODE>" + Header);
		// 	
		// 	Assert.That(children.Count, Is.EqualTo(1));
		// }

	}
}