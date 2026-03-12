using UnityEngine;
using VSEngine;
using XNode;

namespace Core.VSEngine
{
    public abstract class BasicFlowNode : BasicExecutableNode
    {
        [Node.Output(Node.ShowBackingValue.Never, Node.ConnectionType.Override), SerializeField] 
        private Control Continue;
        
        protected abstract void Action();
        
        public override void Execute()
        {
            Action();
            ContinueWith(nameof(Continue));
        }
    }
}