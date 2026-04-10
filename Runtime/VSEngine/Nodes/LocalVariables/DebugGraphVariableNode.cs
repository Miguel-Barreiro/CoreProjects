using Core.Utils;
using UnityEngine;
using XNode;

namespace Core.VSEngine.Nodes.LocalVariables
{
	[Node.CreateNodeMenu(VSNodeMenuNames.TEST_MENU+"/Debug Graph Variable", order = VSNodeMenuNames.IMPORTANT)]
	[Node.NodeTint(VSNodeMenuNames.DEBUG_NODES_TINT)]
	public sealed class DebugGraphVariableNode : BasicFlowNode
	{
		
		[SerializeField]
		private bool Pause = false;
		[SerializeField]
		[TextArea(2, 10)]
		private string Header;

		[SerializeField]
		private string VariableName;

		
		protected override void Action()
		{
			if (Pause)
			{
				Debug.Log($"PAUSE");
				Debug.DebugBreak();
			}

			OperationResult<object> variableValueResult = ScriptExecution!.GetGraphVariable(VariableName);
			if (variableValueResult.IsFailure)
			{
				Debug.Log($"VS: {name}: [{Header}]: variable {VariableName} not found in graph {graph.name}");    
				return;
			}

			Debug.Log($"VS {name}: [{Header}]: variable {VariableName} = {variableValueResult.Result}");
		}
		
		
		public override OperationResult<object> GetValue(string portName)
		{
			throw new System.NotImplementedException();
		}
	}
}