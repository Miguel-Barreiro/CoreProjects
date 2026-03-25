using Core.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.VSEngine.Nodes
{
    [NodeWidth(300)]
    [CreateNodeMenu(VSNodeMenuNames.MATH_MENU+"/"+ VSNodeMenuNames.VALUES_MENU +"/String", order = 2)]
    [NodeTint(VSNodeMenuNames.VALUES_NODES_TINT)]
    public class StringNode : ValueOnlyNode
    {
        [Output(ShowBackingValue.Always), SerializeField, HideLabel]
        [TextArea(3, 10)]
        private string value;
        

        public override OperationResult<object> GetValue(string portName)
        {
            return OperationResult<object>.Success(value);
            
        }
    }
}