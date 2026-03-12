using Core.Utils;
using FixedPointy;
using UnityEngine;

namespace Core.VSEngine.TestNodes
{
    
    [CreateNodeMenu("Miguel/testing/ Fix ValueOnly Node", order = 2)]
    public class TestValueOnlyNode : ValueOnlyNode 
    {
        [Output(ShowBackingValue.Always), SerializeField]
        private Fix value;
        
        public override OperationResult<object> GetValue(string portName)
        {
            return OperationResult<object>.Success(value);
        }
    }
}