using Core.VSEngine;
using UnityEngine;
using UnityEngine.Serialization;
using VSEngine;
using XNode;

namespace Core.VSEngine
{
    [Node.CreateNodeMenu("Flow/Multiple Paths ")]
    public class MultipleBranchTestNode : BasicExecutableNode
    {
        [Node.Output(Node.ShowBackingValue.Never, Node.ConnectionType.Override, Node.TypeConstraint.Strict, dynamicPortList = true), SerializeField]
        private Control?[] Continue = null;
    
        public override void Execute()
        {
            MultipleContinue(nameof(Continue));
        }
    }
}