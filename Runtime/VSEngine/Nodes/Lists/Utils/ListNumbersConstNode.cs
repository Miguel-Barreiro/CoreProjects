using System;
using System.Collections.Generic;
using Core.Utils;
using FixedPointy;
using UnityEngine;
using XNode;

namespace Core.VSEngine.Nodes.Lists.Utils
{
	[Node.CreateNodeMenu(VSNodeMenuNames.VALUES_MENU+"/New List [NUMBERS]", order = VSNodeMenuNames.IMPORTANT)]
	[Node.NodeTint(VSNodeMenuNames.VALUES_NODES_TINT)]
	[Node.NodeWidth(140)]
	[Serializable]
	public class ListNumbersConstNode : ValueOnlyNode
	{
		[SerializeField] private List<float> Values;

		private const string RESULT_LIST_PORT_NAME = "Result";

		public override OperationResult<object> GetValue(string portName)
		{
			List<Fix> result = new List<Fix>();
			foreach (float f in Values)
				result.Add((Fix)f);
			return SUCCESS_RETURN<object>(result);
		}
		
#if UNITY_EDITOR
        
        
		public override void OnBeforeSerialize()
		{
			Type loopListType = NodeUtils.GetListTypeFromElementType(typeof(Fix));
			
			NodePort? resultPort = GetOutputPort(RESULT_LIST_PORT_NAME);
			if (resultPort != null && resultPort.ValueType != loopListType)
			{
				RemoveDynamicPort(RESULT_LIST_PORT_NAME);
				resultPort = null;
			}
			if (resultPort == null)
				AddDynamicOutput(loopListType, fieldName: RESULT_LIST_PORT_NAME,
								typeConstraint: TypeConstraint.Strict, connectionType: ConnectionType.Multiple);
            
            
			base.OnBeforeSerialize();
		}
#endif        

	}
}