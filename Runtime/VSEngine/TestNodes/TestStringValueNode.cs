using Core.Utils;
using UnityEngine;

namespace Core.VSEngine.TestNodes
{
    [NodeWidth(300)]
    [CreateNodeMenu("Miguel/testing/String ValueOnly Node", order = 2)]
    public class TestStringValueNode : ValueOnlyNode
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