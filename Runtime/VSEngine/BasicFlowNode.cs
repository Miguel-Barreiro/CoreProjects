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
            Action();
            ContinueWith(nameof(Continue));
        }

        public abstract OperationResult<object> GetValue(string portName);
    }
}