using Core.VSEngine;
using UnityEngine;
using XNode;

namespace VSEngine
{
    public abstract class BasicExecutableNode : ExecutableNode
    {
        [Node.Input(Node.ShowBackingValue.Never, Node.ConnectionType.Multiple)]
        [SerializeField]
        private Control? enter = null;
    }
}