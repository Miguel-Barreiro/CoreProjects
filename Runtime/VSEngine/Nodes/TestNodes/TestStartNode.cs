using System;
using Core.Utils;
using UnityEngine;
using XNode;

namespace Core.VSEngine.Nodes.TestNodes
{
	[Node.NodeWidth(200)]
	[Node.CreateNodeMenu(VSNodeMenuNames.TEST_MENU+"/Test Start", order = VSNodeMenuNames.IMPORTANT)]
	[Node.NodeTint(VSNodeMenuNames.DEBUG_TEST_NODES_TINT)]
	[Serializable]
	public sealed class TestStartNode : BasicFlowNode
	{
		[SerializeField, Space(10)]
		private int repeatNumber = 1;

		public int RepeatNumber => repeatNumber;

		protected override void Action()
		{
			Debug.Log($"Starting {name} test");	
		}
		
		public override OperationResult<object> GetValue(string portName)
		{
			return OperationResult<object>.Failure($"{name} [{GetType().Name}] in {graph.name} should not have any value output ports");
		}
	}
}