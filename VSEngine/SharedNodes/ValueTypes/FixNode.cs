#nullable enable

using FixedPointy;
using UnityEngine;
using XNode;

namespace Core.VSEngine.SharedNodes.ValueTypes
{
    public class FixNode : Node, ISharedNode
    {
        public Node Node => this;

        [Input(ShowBackingValue.Always, ConnectionType.Override)]
        [SerializeField]
        private Fix input;

        [Output(ShowBackingValue.Never, ConnectionType.Multiple)]
        [SerializeField]
        private Fix output;
        
        object? ISharedNode.ResolveValue(string fieldName)
        {
            return input;
        }

#if UNITY_EDITOR
        public void TEST_SetInput(Fix input)
        {
            this.input = input;
        }
#endif
    }
}
