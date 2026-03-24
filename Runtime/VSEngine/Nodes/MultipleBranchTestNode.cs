using UnityEngine;
using VSEngine;
using XNode;

namespace Core.VSEngine.Nodes
{
    
    [CreateNodeMenu(MenuNames.FLOW_MENU+"/Multiple Paths", order = 2)]
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