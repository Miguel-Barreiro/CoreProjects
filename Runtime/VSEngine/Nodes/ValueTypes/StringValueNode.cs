using Core.Utils;
using UnityEngine;

namespace Core.VSEngine.Nodes
{
    [NodeWidth(300)]
    [CreateNodeMenu(MenuNames.MATH_MENU+"/"+ MenuNames.VALUES_MENU +"/String", order = 2)]
    public class StringValueNode : ValueOnlyNode
    {
        [Output(ShowBackingValue.Always), SerializeField]
        [TextArea(3, 10)]
        private string value;
        

        public override OperationResult<object> GetValue(string portName)
        {
            return OperationResult<object>.Success(value);
            
        }
    }
}