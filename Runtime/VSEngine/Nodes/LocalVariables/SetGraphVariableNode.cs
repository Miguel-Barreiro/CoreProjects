
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
		[SerializeField] private string VariableName;
		
		[SerializeField, Space(10)]
		private NodeElementType Type = NodeElementType.Numbers;


		protected override void Action()
		{
			if(Check(string.IsNullOrEmpty(VariableName), $"variable name for {name} {nameof(SetGraphVariableNode)} is empty"))
				return;

			OperationResult<object> operationResult = GetNewValue();
			if (operationResult.IsSuccess)
				ScriptExecution!.SetGraphVariable(VariableName, operationResult.Result);

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
		
		private const string VAR_VALUE_PORT_NAME = "Value";
		
		        
#if UNITY_EDITOR
        
		public override void OnBeforeSerialize()
		{
			Type loopType = ElementTypeExtensions.GetLogicType(Type);
            
			NodePort? executePort = GetInputPort(VAR_VALUE_PORT_NAME);
			if (executePort != null && executePort.ValueType != loopType)
			{
				RemoveDynamicPort(VAR_VALUE_PORT_NAME);
				executePort = null;
			}
			if (executePort == null)
			{
				AddDynamicInput(loopType, fieldName: VAR_VALUE_PORT_NAME,
								typeConstraint: TypeConstraint.Strict,
								connectionType: Node.ConnectionType.Override);
			}
			base.OnBeforeSerialize();
		}
		public override void OnAfterDeserialize() { }
#endif

	}
}
