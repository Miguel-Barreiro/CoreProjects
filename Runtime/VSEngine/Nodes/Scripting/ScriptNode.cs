#nullable enable

using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace Core.VSEngine.Nodes.Scripting {

    
    [Node.CreateNodeMenu(VSNodeMenuNames.FLOW_MENU+"/" + VSNodeMenuNames.NESTED_MENU +"/Script ", order = 2)]
    [NodeTint(VSNodeMenuNames.SCRIPT_TINT)]
    [NodeWidth(600)]
    public sealed class ScriptNode : Node
    {
        // We use these suffixes so that inputs/outputs can have the same name
        // xNode maps by fieldName which would conflict with the other port
        private const string INPUT_PARAMETER_SUFFIX = " [I]";
        private const string OUTPUT_PARAMETER_SUFFIX = " [O]";

        [Input(ShowBackingValue.Never)]
        [SerializeField]
        private Control? enter = null;

        [Output(ShowBackingValue.Never)]
        [SerializeField]
        private Control? exit = null;

        [Input(ShowBackingValue.Always, ConnectionType.Override)]
        [SerializeField]
        private ActionGraph? script = null;
        public ActionGraph? Script => script;
        
        private OutputsNode? outputsNode = null;
        private InputsNode? inputsNode = null;

        private void AddInputParameterPort(SerializedTypeParameter output) {
            AddDynamicInput(
                output.SerializedType.GetParsedType(), 
                ConnectionType.Override, 
                TypeConstraint.Strict,
                fieldName: output.ParameterName + INPUT_PARAMETER_SUFFIX 
                // , hideLabel: false
            );
        }

        private void AddOutputParameterPort(SerializedTypeParameter output) {
            AddDynamicOutput(
                output.SerializedType.GetParsedType(), 
                ConnectionType.Multiple, 
                TypeConstraint.Strict,
                fieldName: output.ParameterName + OUTPUT_PARAMETER_SUFFIX 
                // , hideLabel: false
            );
        }
        
        
        private void UpdateParameterPort(SerializedTypeParameter parameter, NodePort nodePort)
        {
            nodePort.ValueType = parameter.SerializedType.GetParsedType();
        }
        
        private void OnValidate()
        {
            if (script == null) {
                ClearDynamicPorts();

                outputsNode = null;
                inputsNode = null;

                return;
            }

            // TODO: update ports in a smarter way so that valid connections aren't severed unintuitively

            // ClearDynamicPorts();

            Dictionary<string, NodePort> dynamicPortsByName = new();
            List<NodePort> dynamicPorts = new List<NodePort>(DynamicPorts);
            foreach (NodePort port in dynamicPorts) {
                dynamicPortsByName.Add(port.fieldName, port);
            }
            
            foreach (Node node in script.nodes) 
            {
                if (node is OutputsNode outputsNode) 
                {
                    this.outputsNode = outputsNode;
                } else if (node is InputsNode inputsNode) {
                    this.inputsNode = inputsNode;
                }
            }

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
            
        }

        // Node ISharedNode.Node => this;

        /// <summary>
        /// Resolve all input ports and pass them onto the subscript
        /// </summary>
        // private void PrepareInputs() 
        // {
        //     if (inputsNode != null) 
        //     {
        //         foreach (NodePort? port in DynamicInputs) 
        //         {
        //             if (port == null) {
        //                 Debug.LogError($"null port on {nameof(ScriptNode)} in {graph.name}");
        //
        //                 continue;
        //             }
        //
        //             object? value = ActionGraphUtil.ResolvePortValue(port);
        //
        //             inputsNode.SetValue(port.fieldName.Replace(INPUT_PARAMETER_SUFFIX, ""), value);
        //         }
        //     }
        // }

        /// <summary>
        /// Execute subscript
        /// </summary>
        // void ISharedNode.ResolveControl(ICollection<Node> nextNodes)
        // {
        //     PrepareInputs();
        //
        //     if (entryNode != null) {
        //         ActionGraphUtil.ResolveNodeChain(entryNode.Node);
        //     }
        //
        //     Node? nextNode = GetNodeFromPort(nameof(exit));
        //
        //     if (nextNode == null) {
        //         return;
        //     }
        //
        //     nextNodes.Add(nextNode);
        // }
        
        /// <summary>
        /// Execute subscript
        /// </summary>
        // async UniTask ISharedNode.ResolveControlAsync(ICollection<Node> nextNodes)
        // {
        //     PrepareInputs();
        //
        //     if (entryNode != null) {
        //         await ActionGraphUtil.ResolveNodeChainAsync(entryNode.Node);
        //     }
        //
        //     Node? nextNode = GetNodeFromPort(nameof(exit));
        //
        //     if (nextNode == null) {
        //         return;
        //     }
        //
        //     nextNodes.Add(nextNode);
        // }
        //
        // /// <summary>
        // /// Retrieve values from associated OutputsNode
        // /// </summary>
        // object? ISharedNode.ResolveValue(string fieldName)
        // {
        //     string fieldNameWithoutSuffix = fieldName.Replace(OUTPUT_PARAMETER_SUFFIX, "");
        //
        //     if (outputsNode == null || !outputsNode.HasValue(fieldNameWithoutSuffix)) {
        //         return null;
        //     }
        //
        //     return outputsNode.GetValue(fieldNameWithoutSuffix);
        // }
        //
        // /// <summary>
        // /// This isn't used anywhere, it's probably just here to suppress xNode warnings.
        // /// </summary>
        // public override object? GetValue(NodePort port)
        // {
        //     return null;
        // }

#if UNITY_EDITOR
        public void TEST_Validate() 
        {
            OnValidate();
        }

        public void TEST_SetScript(ActionGraph script)
        {
            this.script = script;
        }
#endif
    }
}