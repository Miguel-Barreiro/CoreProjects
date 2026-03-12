using UnityEngine;
using XNode;
using Core.VSEngine;

namespace Core.VSEngine.NestedVisualScripting
{
    [Node.CreateNodeMenu("Nested/Start", order = 2)]
    [Node.NodeTint("#194d33")]
    public class StartVSNode : ExecutableNode
    {
        [Node.Output(Node.ShowBackingValue.Never)]
        [SerializeField]
        private Control? exit = null;
        
        public override void Execute()
        {
            ContinueWith(nameof(exit));
        }
    }
}