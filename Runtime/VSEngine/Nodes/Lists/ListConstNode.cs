using System;
using System.Collections;
using Core.Utils;
using UnityEngine;
using XNode;

namespace Core.VSEngine.Nodes.Lists
{

	[Node.CreateNodeMenu(VSNodeMenuNames.VALUES_MENU+"/New List", order = VSNodeMenuNames.IMPORTANT)]
	[Node.NodeTint(VSNodeMenuNames.VALUES_NODES_TINT)]
	public sealed class ListConstNode : ValueOnlyNode
	{
		[SerializeField]
		private NodeElementType ElementType = NodeElementType.Entities;

		[SerializeField, Range(0, MAX_ELEMENTS)]
		[Space]
		private int NumberElements = 1;

		private const string RESULT_LIST_PORT_NAME = "Result";
		private const string INPUTS_PORT_NAME = "Input";
		private const int MAX_ELEMENTS = 20;
		
		public override OperationResult<object> GetValue(string portName)
		{
			return SUCCESS_RETURN<object>(GetListFromInputs());
		}

		private static readonly string[] InputNames = new string[MAX_ELEMENTS]
		{
			"Input0", "Input1", "Input2", "Input3", "Input4", "Input5", "Input6", "Input7", "Input8", "Input9",
			"Input10", "Input11", "Input12", "Input13", "Input14", "Input15", "Input16", "Input17", "Input18", "Input19",
		};

		private IList GetListFromInputs()
		{
			Type elementsType = ElementType.GetLogicType();
			Type elementsListType = NodeUtils.GetListTypeFromElementType(elementsType);
			IList resultList = (IList)Activator.CreateInstance(elementsListType);

			for (int i = 0; i < NumberElements; i++)
			{
				string inputsPortName = InputNames[i];
				OperationResult<object> operationResult = ResolveDynamic(inputsPortName);
				if (Check(operationResult.IsFailure, $"Failed to resolve input {i}"))
					continue;
				resultList.Add(operationResult.Result);
			}
			
			return resultList;
		}


#if UNITY_EDITOR
        
        
        public override void OnBeforeSerialize()
        {
            Type elementsType = ElementType.GetLogicType();
            Type loopListType = NodeUtils.GetListTypeFromElementType(elementsType);
			
			for (int i = 0; i < MAX_ELEMENTS; i++)
			{
				string inputsPortName = InputNames[i];
				NodePort? inputPort = GetInputPort(inputsPortName);
	            if (inputPort != null && inputPort.ValueType != elementsType)
	            {
	                RemoveDynamicPort(inputsPortName);
	                inputPort = null;
	            }
	            if (inputPort == null && i < NumberElements)
	                AddDynamicInput(elementsType, 
									fieldName: inputsPortName,
									typeConstraint: TypeConstraint.Strict,
									connectionType: ConnectionType.Override);
			}
            
            
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