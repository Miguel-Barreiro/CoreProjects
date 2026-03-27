using System;
using System.Collections;
using System.Collections.Generic;
using Core.Utils;
using UnityEngine;
using XNode;

namespace Core.VSEngine.Nodes.Lists
{
    [Node.CreateNodeMenu(VSNodeMenuNames.LIST_MENU+"/Filter List", order = VSNodeMenuNames.IMPORTANT)]
    [Node.NodeTint(VSNodeMenuNames.LIST_NODES_TINT)]
    [NodeWidth(300)]
    public class FilterListByConditionNode : ValueOnlyNode
    {
        [SerializeField]
        private NodeElementType Type = NodeElementType.Entities;

        [SerializeField]
        private bool cacheResult = true;
        
        private const string LOOP_ELEMENT_PORT_NAME = "CurrentElement";
        private const string LOOP_LIST_PORT_NAME = "List";
        private const string RESULT_LIST_PORT_NAME = "ResultList";
        private const string SHOULD_INCLUDE_PORT_NAME = "ShouldInclude";

        private const string CURRENT_CONDITION_ENTITY_VARIABLE = "CURRENT_CONDITION_ENTITY";

        public override OperationResult<object> GetValue(string portName)
        {
            return portName switch
            {
                RESULT_LIST_PORT_NAME => GetFilteredList(),
                LOOP_ELEMENT_PORT_NAME => GetCurrentConditionEntity(),
                
                _=> INVALID_EXECUTION<object>($"No output with name {portName} found in {name}")
            };
        }

        private OperationResult<object> GetCurrentConditionEntity()
        {
            return GetVariable(CURRENT_CONDITION_ENTITY_VARIABLE);
        }

        private OperationResult<object> GetFilteredList()
        {
            if(cacheResult && TryCache(out object? result))
            {
                return OperationResult<object>.Success(result);
            }

            OperationResult<IList> originalListResult = GetOriginalList();
            if (originalListResult.IsFailure)
                return INVALID_INPUT<object>(nameof(LOOP_LIST_PORT_NAME));
            
            
            IList? filteredList = FilterByCondition(originalListResult.Result);

            if (cacheResult)
                CacheResult(filteredList);
            
            return OperationResult<object>.Success(filteredList);
        }
        
        private IList FilterByCondition(IList entities)
        {
            Type elementsType = Type.GetLogicType();
            Type elementsListType = NodeUtils.GetListTypeFromElementType(elementsType);
            IList resultList = (IList)Activator.CreateInstance(elementsListType);
            
            foreach (object element in entities)
                if (CheckCondition(element))
                    resultList.Add(element);

            return resultList;
        }
        

        private bool CheckCondition(object element)
        {
            SetVariable(CURRENT_CONDITION_ENTITY_VARIABLE, element);
            OperationResult<object> doesEntityPassRes = ResolveDynamic(SHOULD_INCLUDE_PORT_NAME);
            SetVariable(CURRENT_CONDITION_ENTITY_VARIABLE, null);
            
            if(doesEntityPassRes.IsFailure)
                return false;

            return (bool) doesEntityPassRes.Result;
        }

        private const string NO_LIST_GIVEN_NULL_MSG = "FilterListByConditionNode was not given a list";
        private const string ORIGINAL_LIST_IS_NULL_MSG = "Original list is null";
        private OperationResult<IList?> GetOriginalList()
        {
            OperationResult<object> listValue = ResolveDynamic(LOOP_LIST_PORT_NAME);
            if(Check(listValue.IsFailure, ORIGINAL_LIST_IS_NULL_MSG))
                return OperationResult<IList?>.Failure(ORIGINAL_LIST_IS_NULL_MSG);
            
            IList? list = listValue.Result as IList;
            if(Check(list == null, NO_LIST_GIVEN_NULL_MSG))
                return OperationResult<IList?>.Failure(NO_LIST_GIVEN_NULL_MSG);

            return OperationResult<IList?>.Success(list);
        }


#if UNITY_EDITOR
        
        
        public override void OnBeforeSerialize()
        {
            Type elementsType = Type.GetLogicType();
            Type loopListType = NodeUtils.GetListTypeFromElementType(elementsType);
            
            NodePort? listPort = GetInputPort(LOOP_LIST_PORT_NAME);
            if (listPort != null && listPort.ValueType != loopListType)
            {
                RemoveDynamicPort(LOOP_LIST_PORT_NAME);
                listPort = null;
            }
            if (listPort == null)
            {
                AddDynamicInput(loopListType, fieldName: LOOP_LIST_PORT_NAME,
                    typeConstraint: TypeConstraint.Strict, connectionType: ConnectionType.Override);
            }
            
            NodePort? elementPort = GetOutputPort(LOOP_ELEMENT_PORT_NAME);
            if (elementPort != null && elementPort.ValueType != elementsType)
            {
                RemoveDynamicPort(LOOP_ELEMENT_PORT_NAME);
                elementPort = null;
            }
            if (elementPort == null)
            {
                AddDynamicOutput(elementsType, fieldName: LOOP_ELEMENT_PORT_NAME,
                    typeConstraint: TypeConstraint.Strict, connectionType: ConnectionType.Multiple);
            }
            
            NodePort? conditionPort = GetInputPort(SHOULD_INCLUDE_PORT_NAME);
            if (conditionPort != null && conditionPort.ValueType != typeof(bool))
            {
                RemoveDynamicPort(RESULT_LIST_PORT_NAME);
                conditionPort = null;
            }
            if (conditionPort == null)
            {
                AddDynamicInput(typeof(bool), fieldName: SHOULD_INCLUDE_PORT_NAME,
                    typeConstraint: TypeConstraint.Strict, connectionType: ConnectionType.Override);
            }
            
            NodePort? resultPort = GetOutputPort(RESULT_LIST_PORT_NAME);
            if (resultPort != null && resultPort.ValueType != loopListType)
            {
                RemoveDynamicPort(RESULT_LIST_PORT_NAME);
                resultPort = null;
            }
            if (resultPort == null)
            {
                AddDynamicOutput(loopListType, fieldName: RESULT_LIST_PORT_NAME,
                    typeConstraint: TypeConstraint.Strict, connectionType: ConnectionType.Multiple);
            }
            
            
            base.OnBeforeSerialize();
        }
#endif        
    }   
}
