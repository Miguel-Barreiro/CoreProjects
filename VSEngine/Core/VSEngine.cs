using VSEngine;
using XNode;

namespace Core.VSEngine
{
    public class VSEngine : VSEngineCore
    {
        public void Run(NodeGraph nodeGraph, BaseEventListenNode eventListenNode, VSEventBase vsEvent)
        {
            RunInternal(nodeGraph, eventListenNode, vsEvent, new VSExecutionControl());
        }
    }
}
