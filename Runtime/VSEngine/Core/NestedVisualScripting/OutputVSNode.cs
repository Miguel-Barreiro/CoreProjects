#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Core.Utils;
using Core.Utils.CachedDataStructures;
using Core.VSEngine.Nodes;
using UnityEngine;
using XNode;

namespace Core.VSEngine.NestedVisualScripting
{
    
    
    [Node.CreateNodeMenu(VSNodeMenuNames.FLOW_MENU+"/" + VSNodeMenuNames.NESTED_MENU +"/"+ VSNodeMenuNames.NESTED_TITLE+ "Output", order = VSNodeMenuNames.LOW)]
    [NodeTint(VSNodeMenuNames.SCRIPT_TINT)]
    [NodeWidth(600)]
    public class OutputVSNode : ValueOnlyNode
    {
        [Input(ShowBackingValue.Always, ConnectionType.Override)]
        [SerializeField]
        private List<SerializedTypeParameter> outputTypes = new();
        public ReadOnlyCollection<SerializedTypeParameter> OutputTypes => outputTypes.AsReadOnly();
        
        public override OperationResult<object> GetValue(string inputName)
        {
            return Resolve<object>(inputName);
        }

        public void SetOutputValues(Dictionary<string,object?> outputs)
        {
            foreach (SerializedTypeParameter output in outputTypes)
            {
                outputs.Add(output.ParameterName, GetValue(output.ParameterName));
            }
        }

        
#if UNITY_EDITOR

        public override void OnBeforeSerialize()
        {
            // Prevent null items in list
            for (int i = 0; i < outputTypes.Count; i++) {
                if ((outputTypes[i] as object) == null) {
                    outputTypes[i] = new SerializedTypeParameter();
                }
            }
            
            // Add missing input ports
            foreach (SerializedTypeParameter output in outputTypes) {
                if (!output.IsValid()) {
                    continue;
                }

                if (!HasPort(output.ParameterName)) {
                    AddParameterPort(output);
                }
            }

            using CachedList<NodePort> dynamicInputs = ListCache<NodePort>.Get();
            dynamicInputs.AddRange(DynamicInputs);

            foreach (NodePort port in dynamicInputs) {
                SerializedTypeParameter? output = GetParameter(port.fieldName);

                if (output == null)
                {
                    RemoveDynamicPort(port);

                    continue;
                }

                Type outputType = output.Value.SerializedType.GetParsedType();
                
                // Update port type
                if (port.ValueType != outputType) {
                    port.ValueType = outputType;

                    port.ClearConnections();
                }
            }
            
            base.OnBeforeSerialize();
        }
        
        private SerializedTypeParameter? GetParameter(string outputName) {
            foreach (SerializedTypeParameter output in outputTypes) {
                if (output.ParameterName != outputName) {
                    continue;
                }

                return output;
            }

            return null;
        }
        
        private void AddParameterPort(SerializedTypeParameter output) {
            AddDynamicInput(
                output.SerializedType.GetParsedType(), 
                ConnectionType.Override, 
                TypeConstraint.Strict,
                fieldName: output.ParameterName 
                // , hideLabel: false
            );
        }

#endif

    }
}