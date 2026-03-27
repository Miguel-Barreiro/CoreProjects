using UnityEngine;
using VSEngine;
using XNode;

namespace Core.VSEngine.Nodes
{
    
    [CreateNodeMenu(VSNodeMenuNames.FLOW_MENU+"/Multiple Paths", order = VSNodeMenuNames.IMPORTANT)]
    [NodeTint(VSNodeMenuNames.FLOW_NODES_TINT)]
    public class MultipleBranchNode : BasicExecutableNode
    {
        [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict, dynamicPortList = true), SerializeField]
        private Control?[] Continue = null;
    
        public override void Execute()
        {
            MultipleContinue(nameof(Continue));
        }
    }
}