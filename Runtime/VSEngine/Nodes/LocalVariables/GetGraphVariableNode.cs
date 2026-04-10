using System;
using Core.Utils;
using UnityEngine;
using XNode;

namespace Core.VSEngine.Nodes.LocalVariables
{
	
	[CreateNodeMenu(VSNodeMenuNames.VALUES_MENU +"/Graph Variable", order = VSNodeMenuNames.IMPORTANT)]
	[NodeTint(VSNodeMenuNames.VARIABLE_NODES_TINT)]
	public class GetGraphVariableNode : ValueOnlyNode
	{
		[SerializeField] private string VariableName;
		
		[SerializeField, Space(10)]
		private NodeElementType Type = NodeElementType.Numbers;

		
		public override OperationResult<object> GetValue(string portName)
		{
			string errorMessage = $"variable name for {name} {nameof(GetGraphVariableNode)} is empty";

			if(Check(string.IsNullOrEmpty(VariableName), errorMessage))
				return OperationResult<object>.Failure(errorMessage);

			return ScriptExecution!.GetGraphVariable(VariableName);
		}
		
		private const string VAR_VALUE_PORT_NAME = "Value";
		
		        
#if UNITY_EDITOR
        
		public override void OnBeforeSerialize()
		{
			Type loopType = ElementTypeExtensions.GetLogicType(Type);
            
			NodePort? executePort = GetOutputPort(VAR_VALUE_PORT_NAME);
			if (executePort != null && executePort.ValueType != loopType)
			{
				RemoveDynamicPort(VAR_VALUE_PORT_NAME);
				executePort = null;
			}
			if (executePort == null)
			{
				AddDynamicOutput(loopType, fieldName: VAR_VALUE_PORT_NAME,
								typeConstraint: TypeConstraint.Strict, connectionType: Node.ConnectionType.Multiple);
			}
			base.OnBeforeSerialize();
		}
		public override void OnAfterDeserialize() { }
#endif

	}
}
