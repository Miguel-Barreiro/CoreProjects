
using System;
using Core.Utils;
using UnityEngine;
using XNode;

namespace Core.VSEngine.Nodes.LocalVariables
{
	
	[CreateNodeMenu(VSNodeMenuNames.VALUES_MENU +"/Set Graph Variable", order = VSNodeMenuNames.IMPORTANT)]
	[NodeTint(VSNodeMenuNames.VARIABLE_WRITE_NODES_TINT)]
	public sealed class SetGraphVariableNode : BasicFlowNode
	{
		[SerializeField] private int VariableIndex = 1;
		
		// [SerializeField, Space(10)]
		// private NodeElementType Type = NodeElementType.Numbers;


		protected override void Action()
		{
			string errorMessage = $"Variable index {VariableIndex} is out of range({VariableIndex}) for node {name} in graph {graph.name} ";
			ActionGraph actionGraph = graph as ActionGraph;
			if(Check(VariableIndex < 0 || VariableIndex >= actionGraph!.LocalVariables.Count, errorMessage))
				return;
			LocalVariableDefinition graphVariable = actionGraph!.LocalVariables[VariableIndex];
			
			OperationResult<object> operationResult = GetNewValue();
			if (operationResult.IsSuccess)
				ScriptExecution!.SetGraphVariable(graphVariable.Name, operationResult.Result);

		}

		public override OperationResult<object> GetValue(string portName)
		{
			throw new NotImplementedException();
		}

		public OperationResult<object> GetNewValue()
		{
			OperationResult<object> operationResult = Resolve<object>(VAR_VALUE_PORT_NAME);
			return operationResult;

		}
		
		public const string VAR_VALUE_PORT_NAME = "Value";
		
		        
#if UNITY_EDITOR
        
		public override void OnBeforeSerialize()
		{
			ActionGraph actionGraph = graph as ActionGraph;
			if (VariableIndex < 0 || VariableIndex >= actionGraph.LocalVariables.Count)
			{
				Debug.LogError($"Variable index {VariableIndex} is out of range({VariableIndex}) for node {name} in graph {graph.name} ");
				return;
			}

			LocalVariableDefinition graphVariable = actionGraph!.LocalVariables[VariableIndex];
			Type variableType = ElementTypeExtensions.GetLogicType(graphVariable.Type);
            
			NodePort? executePort = GetInputPort(VAR_VALUE_PORT_NAME);
			if (executePort != null && executePort.ValueType != variableType)
			{
				RemoveDynamicPort(VAR_VALUE_PORT_NAME);
				executePort = null;
			}
			if (executePort == null)
			{
				AddDynamicInput(variableType, fieldName: VAR_VALUE_PORT_NAME,
								typeConstraint: TypeConstraint.Strict,
								connectionType: Node.ConnectionType.Override);
			}
			base.OnBeforeSerialize();
		}
		public override void OnAfterDeserialize() { }
#endif

	}
}
