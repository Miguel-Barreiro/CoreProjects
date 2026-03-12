#nullable enable

using Core.Utils;
using FixedPointy;
using UnityEngine;
using XNode;

namespace Core.VSEngine.SharedNodes.ValueTypes
{
    public class FixNode : ValueOnlyNode
    {
        public Node Node => this;

        [SerializeField]
        private float Value;

        [Output(ShowBackingValue.Never, ConnectionType.Multiple)]
        [SerializeField]
        private Fix output;
        
        public override OperationResult<object> GetValue(string portName)
        {
            return OperationResult<object>.Success((Fix)Value);
        }
    }
}
