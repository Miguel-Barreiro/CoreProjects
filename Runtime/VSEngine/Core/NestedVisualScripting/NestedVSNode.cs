#nullable enable

using System.Collections.Generic;
using Core.Utils;
using Core.VSEngine.Nodes;
using Sirenix.OdinInspector;
using UnityEngine;
using VSEngine;
using VSEngine.Core.NestedVisualScripting;
using XNode;

namespace Core.VSEngine.NestedVisualScripting
{
    [Node.CreateNodeMenu(VSNodeMenuNames.FLOW_MENU+"/" + VSNodeMenuNames.NESTED_MENU +"/"+ VSNodeMenuNames.NESTED_TITLE+ "Script", order = VSNodeMenuNames.LOW)]
    [NodeTint(VSNodeMenuNames.SCRIPT_TINT)]
    [NodeWidth(600)]
    public sealed class NestedVSNode : BasicFlowNode, ICacheOutputValues
    {
        // We use these suffixes so that inputs/outputs can have the same name
        // xNode maps by fieldName which would conflict with the other port
        private const string INPUT_PARAMETER_SUFFIX = " [I]";
        private const string OUTPUT_PARAMETER_SUFFIX = " [O]";
        
        [Header("[NOT WORKING ATM]")]
        [Node.Input(Node.ShowBackingValue.Always, Node.ConnectionType.Override)]
        [SerializeField]
        private ActionGraph? script = null;
        public ActionGraph? Script => script;
        
        protected override void Action()
        {
            if (script == null)
            {
                Debug.LogError($"Null ActionGraph given for the NestedVSNode in {graph.name}");
                return;
            }

            Dictionary<string, object?> inputValues = new ();
            PrepareInputValues(inputValues);
            ExecutionControl.PushScript(script, this, inputValues);
        }
        
        public override OperationResult<object> GetValue(string portName)
        {
            if(GetVariable(portName, out object? value))
                return OperationResult<object>.Success(value!);

            string message = $"No value was cached in {name} for {portName} in {graph.name}";
            Debug.LogError(message);
            return OperationResult<object>.Failure(message);
        }

        void ICacheOutputValues.CacheOutputValues(Dictionary<string, object?> outputs)
        {
            foreach ((string parameterName, object? value) in outputs)
            {
                string outputParameterName = parameterName + OUTPUT_PARAMETER_SUFFIX;
                SetVariable(outputParameterName, value);
            }
        }
        
     
        /// <summary>
        /// Resolve all input ports and pass them onto the subscript
        /// </summary>
        private void PrepareInputValues(Dictionary<string, object?> inputValues)
        {
            foreach (NodePort? port in DynamicInputs) 
            {
                if (port == null) {
                    Debug.LogError($"null port on {nameof(NestedVSNode)} in {graph.name}");
                    continue;
                }

                object? value = Resolve<object>(port.fieldName);
                inputValues.Add(port.fieldName.Replace(INPUT_PARAMETER_SUFFIX, ""), value);
            }
        }
        
        
#if UNITY_EDITOR
        public override void OnBeforeSerialize()
        {

            if (script == null) {
                ClearDynamicPorts();
                return;
            }

            if (script == graph)
            {
                Debug.LogError($"you cannot nest a graph inside itself in {graph.name}");
                script = null;
                ClearDynamicPorts();
                return;
            }

            // TODO: update ports in a smarter way so that valid connections aren't severed unintuitively

            Dictionary<string, NodePort> dynamicPortsByName = new();
            List<NodePort> dynamicPorts = new List<NodePort>(DynamicPorts);
            foreach (NodePort port in dynamicPorts) {
                dynamicPortsByName.Add(port.fieldName, port);
            }

            OutputVSNode? outputsNode = VSBaseEngine.GetOutputNode(script);
            InputVSNode? inputsNode = VSBaseEngine.GetInputNode(script);

            if (inputsNode != null) {
                foreach (SerializedTypeParameter parameter in inputsNode.InputTypes) {
                    string inputParameterName = parameter.ParameterName + INPUT_PARAMETER_SUFFIX;
                    if (dynamicPortsByName.ContainsKey(inputParameterName))
                    {
                        UpdateParameterPort(parameter, dynamicPortsByName[inputParameterName]);
                        dynamicPortsByName.Remove(inputParameterName);
                    }
                    else
                    {
                        AddInputParameterPort(parameter);
                    }
                }
            }

            if (outputsNode != null) {
                foreach (SerializedTypeParameter parameter in outputsNode.OutputTypes)
                {
                    string outputParameterName = parameter.ParameterName + OUTPUT_PARAMETER_SUFFIX;
                    if (dynamicPortsByName.ContainsKey(outputParameterName))
                    {
                        UpdateParameterPort(parameter, dynamicPortsByName[outputParameterName]);
                        dynamicPortsByName.Remove(outputParameterName);
                    }
                    else
                    {
                        AddOutputParameterPort(parameter);
                    }
                }
            }

            foreach ((string name, NodePort dynamicPort ) in dynamicPortsByName)
            {
                RemoveDynamicPort(dynamicPort);
            }
            
            base.OnBeforeSerialize();
        }
        
        private void AddInputParameterPort(SerializedTypeParameter output) {
            AddDynamicInput(
                output.SerializedType.GetParsedType(), 
                Node.ConnectionType.Override, 
                Node.TypeConstraint.Strict,
                fieldName: output.ParameterName + INPUT_PARAMETER_SUFFIX 
                // , hideLabel: false
            );
        }

        private void AddOutputParameterPort(SerializedTypeParameter output) {
            AddDynamicOutput(
                output.SerializedType.GetParsedType(), 
                Node.ConnectionType.Multiple, 
                Node.TypeConstraint.Strict,
                fieldName: output.ParameterName + OUTPUT_PARAMETER_SUFFIX 
                // , hideLabel: false
            );
        }
        
        private void UpdateParameterPort(SerializedTypeParameter parameter, NodePort nodePort)
        {
            nodePort.ValueType = parameter.SerializedType.GetParsedType();
        }
#endif
        

    }
}