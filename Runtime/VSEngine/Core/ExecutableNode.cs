using UnityEngine;
using XNode;

namespace Core.VSEngine
{
    public abstract class ExecutableNode : VSNodeBase
    {
        public abstract void Execute();

        protected bool ContinueWithAndComeBack(string portName)
        {
            Node? nextNode = GetNodeFromPort(portName);
            if (nextNode == null)
            {
                // Log.LogError($"Node {name} in {graph.name} has no port named {portName}");
                return false;
            }

            if (nextNode is ExecutableNode executableNode)
            {
                ScriptExecution!.PushNode(this);
                ScriptExecution!.PushNode(executableNode);
                return true;
            }
            else
            {
                Debug.LogError($"Node {name} in {graph.name} is not pointing to an executable node {nextNode.name}");
            }
            return false;
        }
        
        
        
    }
}