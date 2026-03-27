using System;
using System.Collections;
using Core.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using XNode;

namespace Core.VSEngine.Nodes
{
    
    [CreateNodeMenu(VSNodeMenuNames.FLOW_MENU+"/ForEach list", order = VSNodeMenuNames.IMPORTANT)]
    [NodeTint(VSNodeMenuNames.FLOW_NODES_TINT)]
    public class ForeachLoopNode : BaseLoopNode, IValueNode
#if UNITY_EDITOR
        , ISerializationCallbackReceiver
#endif
    {
        private const string LOOP_INDEX_VARIABLE = "ForeEachLoopIndex";
        private const string LOOP_EXECUTE_PORT_NAME = "ExecuteValue";
        private const string LOOP_LIST_PORT_NAME = "List";
        
        [Node.Output(Node.ShowBackingValue.Never, Node.ConnectionType.Override), SerializeField] 
        private Control? Continue;

        [SerializeField]
        private NodeElementType Type = NodeElementType.Entities;
        
        [Node.Output(Node.ShowBackingValue.Never, Node.ConnectionType.Override), SerializeField] 
        private Control? LoopExecute;
        
        public OperationResult<object> GetValue(string portName)
        {
            return portName switch
            {
                LOOP_EXECUTE_PORT_NAME => GetLoopValue(),
                _=> INVALID_INPUT_NAME_CALLED<object>(portName),
            };
        }

        private OperationResult<object> GetLoopValue()
        {
            OperationResult<IList> loopList = GetLoopList();
            if (loopList.IsFailure)
                return INVALID_EXECUTION_MESSAGE<object>("List was null");
            
            IList? list = loopList.Result;
            if (list == null)
                return INVALID_EXECUTION_MESSAGE<object>("List was null");

            if (!HasVariable(LOOP_INDEX_VARIABLE))
                return INVALID_EXECUTION_MESSAGE<object>("there was no loop index variable set(likely means the loop isnt executed)");

            OperationResult<object> variable = GetVariable( LOOP_INDEX_VARIABLE);
            int index = (int)variable.Result;
            if (index < list.Count)
                return SUCCESS_RETURN(list![index]);

            return INVALID_EXECUTION_MESSAGE<object>("Loop index was out of range of the list");
        }

        public override void Execute()
        {
            OperationResult<IList> listRes = GetLoopList();
            if (Check(listRes.IsFailure, "couldnt get list"))
                return;

            IList list = listRes.Result;
            if (!HasVariable(LOOP_INDEX_VARIABLE))
            {
                if (list.Count > 0)
                {
                    SetVariable( LOOP_INDEX_VARIABLE, 0);
                    ContinueWithAndComeBack(nameof(LoopExecute));
                }else
                {
                   FinishForEachLoop();
                }
                return;
            }

            OperationResult<object> loopIndexRes = GetVariable( LOOP_INDEX_VARIABLE);
            if (Check(loopIndexRes.IsFailure, "couldnt get loop index"))
                return;
            
            int index = (int)loopIndexRes.Result;

            int newIndex = index + 1;
            if (newIndex < list.Count)
            {
                SetVariable( LOOP_INDEX_VARIABLE, newIndex);
                ContinueWithAndComeBack(nameof(LoopExecute));
            }
            else
            {
                FinishForEachLoop();
            }
        }

        private void FinishForEachLoop()
        {
            RemoveVariables();
            ContinueWith(nameof(Continue));
        }

        private const string FOREACH_ERROR_LISTNULL_MSG = "List is null";
        private const string FOREACH_ERROR_NOLIST_MSG = "Foreach was not given a list";
        private OperationResult<IList> GetLoopList()
        {
            OperationResult<object> listValue = ResolveDynamic(LOOP_LIST_PORT_NAME);
            if(Check(listValue.IsFailure, FOREACH_ERROR_LISTNULL_MSG))
                return INVALID_EXECUTION_MESSAGE<IList>(FOREACH_ERROR_LISTNULL_MSG);
            
            IList? list = listValue.Result as IList;
            if(Check(list == null, FOREACH_ERROR_NOLIST_MSG))
                return INVALID_EXECUTION<IList>(FOREACH_ERROR_NOLIST_MSG);

            return OperationResult<IList>.Success(list);
        }

        
        
        
#if UNITY_EDITOR
        
        public override void OnBeforeSerialize()
        {
            Type loopType = ElementTypeExtensions.GetLogicType(Type);
            
            NodePort? executePort = GetOutputPort(LOOP_EXECUTE_PORT_NAME);
            if (executePort != null && executePort.ValueType != loopType)
            {
                RemoveDynamicPort(LOOP_EXECUTE_PORT_NAME);
                executePort = null;
            }
            if (executePort == null)
            {
                AddDynamicOutput(loopType, fieldName: LOOP_EXECUTE_PORT_NAME,
                    typeConstraint: TypeConstraint.Strict, connectionType: Node.ConnectionType.Multiple);
            }

            Type loopListType = NodeUtils.GetListTypeFromElementType(loopType);
            NodePort? listPort = GetInputPort(LOOP_LIST_PORT_NAME);
            if (listPort != null && listPort.ValueType != loopListType)
            {
                RemoveDynamicPort(LOOP_LIST_PORT_NAME);
                listPort = null;
            }
            if (listPort == null)
            {
                AddDynamicInput(loopListType, fieldName: LOOP_LIST_PORT_NAME,
                    typeConstraint: TypeConstraint.Strict, connectionType: Node.ConnectionType.Override);
            }
            base.OnBeforeSerialize();
        }
        public override void OnAfterDeserialize() { }
#endif

    }
    
}