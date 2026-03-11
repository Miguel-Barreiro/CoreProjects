#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Core.Utils.CachedDataStructures;
using UnityEngine;
using XNode;

namespace Core.VSEngine.NestedVisualScripting
{
    
    [CreateNodeMenu("Nested/Input", order = 2)]
    [NodeWidth(600)]
    [NodeTint("#394734")]
    public class InputVSNode : ValueOnlyNode
    {
        [Input(ShowBackingValue.Always, ConnectionType.Override)]
        [SerializeField]
        private List<SerializedTypeParameter> inputTypes = new();
        public ReadOnlyCollection<SerializedTypeParameter> InputTypes => inputTypes.AsReadOnly();
        
        
        public override object? GetValue(string portName)
        {
            return GetVariable(portName);
        }

        private void AddParameterPort(SerializedTypeParameter input) {
            AddDynamicOutput(
                input.SerializedType.GetParsedType(), 
                ConnectionType.Multiple, 
                TypeConstraint.Strict,
                fieldName: input.ParameterName 
                // , hideLabel: false
            );
        }

        private SerializedTypeParameter? GetParameter(string inputName) {
            foreach (SerializedTypeParameter input in inputTypes) {
                if (input.ParameterName != inputName) {
                    continue;
                }

                return input;
            }

            return null;
        }

        private void OnValidate()
        {
            // Prevent null items in list
            for (int i = 0; i < inputTypes.Count; i++) {
                if ((inputTypes[i] as object) == null) {
                    inputTypes[i] = new SerializedTypeParameter();
                }
            }

            // Add missing output ports
            foreach (SerializedTypeParameter input in inputTypes) {
                // Make sure the user has finished inputting type info
                if (!input.IsValid()) {
                    continue;
                }

                if (!HasPort(input.ParameterName)) {
                    AddParameterPort(input);
                }
            }

            using CachedList<NodePort> dynamicOutputs = ListCache<NodePort>.Get();
            dynamicOutputs.AddRange(DynamicOutputs);

            foreach (NodePort port in dynamicOutputs) {
                SerializedTypeParameter? input = GetParameter(port.fieldName);

                if (input == null)
                {
                    RemoveDynamicPort(port);

                    continue;
                }

                Type inputType = input.Value.SerializedType.GetParsedType();

                // Update port type
                if (port.ValueType != inputType) {
                    port.ValueType = inputType;

                    port.ClearConnections();
                }
            }
        }

        public Node Node => this;


#if UNITY_EDITOR
        public void TEST_SetTypes(IEnumerable<SerializedTypeParameter> inputTypes) 
        {
            this.inputTypes.Clear();
            this.inputTypes.AddRange(inputTypes);
        }

        public void TEST_Validate() 
        {
            OnValidate();
        }
#endif

    }
    
}