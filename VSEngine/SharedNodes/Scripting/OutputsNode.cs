#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Core.Utils.CachedDataStructures;
using Core.Zenject.Source.Internal;
using UnityEngine;
using XNode;

namespace Core.VSEngine.SharedNodes.Scripting
{
    [NodeWidth(600)]
    public class OutputsNode : Node, ISharedNode
    {
        [Input(typeConstraint = TypeConstraint.Strict), SerializeField]
        private Control? enter;

        [Input(ShowBackingValue.Always, ConnectionType.Override)]
        [SerializeField]
        private List<SerializedTypeParameter> outputTypes = new();
        public ReadOnlyCollection<SerializedTypeParameter> OutputTypes => outputTypes.AsReadOnly();

        private Dictionary<string, object?> values = new();
        public IReadOnlyDictionary<string, object?> Values => values;

        public SerializedTypeParameter? GetParameter(string outputName) {
            foreach (SerializedTypeParameter output in outputTypes) {
                if (output.ParameterName != outputName) {
                    continue;
                }

                return output;
            }

            return null;
        }

        public bool HasValue(string inputName)
        {
            return values.ContainsKey(inputName);
        }

        public object? GetValue(string inputName) {
            return values[inputName];
        }

        public void SetValue(string inputName, object? value) {
            values[inputName] = value;
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

        private void OnValidate()
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
        }

        Node ISharedNode.Node => this;

        /// <summary>
        /// Resolve all inputs and set corresponding items in values map
        /// </summary>
        void ISharedNode.ResolveControl(ICollection<Node> nextNodes)
        {
            foreach (NodePort port in DynamicInputs)
            {
                if (!port.IsConnected) {
                    Debug.LogError($"Port {port.fieldName} not connected on {nameof(OutputsNode)} in graph {graph.name}");

                    return;
                }

                NodePort? connectedPort = port.GetConnection(0);

                if (connectedPort == null) {
                    Debug.LogError($"Connected port {port.fieldName} is null");

                    continue;
                }

                SetValue(port.fieldName, this.ResolvePortValue(connectedPort));
            }
        }

#if UNITY_EDITOR
        public void TEST_Validate() 
        {
            OnValidate();
        }
        public void TEST_SetTypes(IEnumerable<SerializedTypeParameter> outputTypes) 
        {
            this.outputTypes.Clear();
            this.outputTypes.AddRange(outputTypes);
        }
#endif
    }
}
