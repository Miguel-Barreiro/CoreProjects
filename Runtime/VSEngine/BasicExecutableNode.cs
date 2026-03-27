using Core.VSEngine;
using UnityEngine;
using UnityEngine.Serialization;
using XNode;

namespace VSEngine
{
    public abstract class BasicExecutableNode : ExecutableNode
    {
        [Node.Input(Node.ShowBackingValue.Never, Node.ConnectionType.Multiple)]
        [SerializeField]
        private Control? Enter = null;
    }
}