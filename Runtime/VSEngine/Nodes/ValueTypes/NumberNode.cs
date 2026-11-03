using Core.Utils;
using FixedPointy;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.VSEngine.Nodes
{
    [CreateNodeMenu(VSNodeMenuNames.VALUES_MENU +"/Number", order = VSNodeMenuNames.IMPORTANT)]
    [NodeWidth(100)]
    public class NumberNode : SimpleValueNode<Fix> 
    {
        [SerializeField, HideLabel] private float ValueFloat;
        
        public override OperationResult<object> GetValue(string portName)
        {
            return OperationResult<object>.Success((Fix)ValueFloat);
        }
    }
}