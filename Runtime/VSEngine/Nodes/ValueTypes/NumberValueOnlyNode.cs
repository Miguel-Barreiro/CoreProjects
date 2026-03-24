using Core.Utils;
using FixedPointy;
using UnityEngine;

namespace Core.VSEngine.Nodes
{
    [CreateNodeMenu(MenuNames.MATH_MENU+"/"+ MenuNames.VALUES_MENU +"/Number", order = 2)]
    public class NumberValueOnlyNode : ValueOnlyNode 
    {
        [Output(ShowBackingValue.Always), SerializeField]
        private Fix value;
        
        public override OperationResult<object> GetValue(string portName)
        {
            return OperationResult<object>.Success(value);
        }
    }
}