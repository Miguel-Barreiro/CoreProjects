using VSEngine;
using XNode;

namespace Core.VSEngine
{
    public sealed class VSBasicEngine : VSEngineCore
    {
        public void Run(NodeGraph nodeGraph, BaseEventListenNode eventListenNode, VSEventBase vsEvent)
        {
            RunInternal(nodeGraph, eventListenNode, vsEvent, new VSExecutionControl());
        }
    }
}
