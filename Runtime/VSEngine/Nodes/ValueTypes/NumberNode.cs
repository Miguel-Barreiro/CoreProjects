using Core.Utils;
using FixedPointy;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.VSEngine.Nodes
{
    [CreateNodeMenu(VSNodeMenuNames.MATH_MENU+"/"+ VSNodeMenuNames.VALUES_MENU +"/Number", order = 2)]
    [NodeTint(VSNodeMenuNames.VALUES_NODES_TINT)]
    [NodeWidth(100)]
    public class NumberNode : ValueOnlyNode 
    {
        [SerializeField, HideLabel] private float ValueFloat;

        [Output(ShowBackingValue.Never, ConnectionType.Multiple, TypeConstraint.Strict), SerializeField]
        private Fix value;
        
        public override OperationResult<object> GetValue(string portName)
        {
            return OperationResult<object>.Success((Fix)ValueFloat);
        }
    }
}