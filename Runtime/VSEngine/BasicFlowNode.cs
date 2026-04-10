using Core.Utils;
using Core.VSEngine.Nodes;
using UnityEngine;
using VSEngine;
using XNode;

namespace Core.VSEngine
{
    [NodeTint(VSNodeMenuNames.FLOW_NODES_TINT)]
    public abstract class BasicFlowNode : BasicExecutableNode, IValueNode
    {
        [Node.Output(Node.ShowBackingValue.Never, Node.ConnectionType.Override), SerializeField] 
        private Control Continue;
        
        protected abstract void Action();
        
        public override void Execute()
        {
            ContinueWith(nameof(Continue));
            Action();
        }

        public abstract OperationResult<object> GetValue(string portName);
    }
}