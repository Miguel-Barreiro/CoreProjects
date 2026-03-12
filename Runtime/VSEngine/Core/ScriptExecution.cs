#nullable enable

using System.Collections.Generic;
using Core.Utils;
using VSEngine.Core.NestedVisualScripting;
using XNode;

namespace Core.VSEngine
{
    public sealed class ScriptExecution
    {
        private readonly HashSet<Node?> injectedNodes = new();

        //this is the node that pushed this script execution
        private ICacheOutputValues? fromNode;
        
        private readonly ActionGraph? nodeGraph;
        public ActionGraph? Graph => nodeGraph;
        public ICacheOutputValues? FromNode => fromNode;
        public ExecutableNode? CurrentNode { set; get; }

        private readonly Stack<ExecutableNode?> NodeExecutionStack = new();
        
        private readonly Dictionary<VSNodeBase, Dictionary<string, object?>> NodeScriptNodeVariables = new();
        private readonly Dictionary<string, object?> NodeScriptExecutionVariables = new();

        public ScriptExecution(ActionGraph nodeGraph, ExecutableNode? startNode, ICacheOutputValues? fromNode = null)
        {
            injectedNodes.Clear();
            NodeExecutionStack.Clear();
            NodeScriptNodeVariables.Clear();
            
            this.nodeGraph = nodeGraph;
            CurrentNode = startNode;
            this.fromNode = fromNode;
        }

        public void PushNode(ExecutableNode? nextExecuteNode)
        {
            NodeExecutionStack.Push(nextExecuteNode);
        }
        public ExecutableNode? PopNode()
        {
            if (NodeExecutionStack.Count == 0)
            {
                CurrentNode = null;
            }
            else
            {
                CurrentNode = NodeExecutionStack.Pop();
            }

            return CurrentNode;
        }
        public bool HasNextNode()
        {
            return NodeExecutionStack.Count > 0;
        }

        public bool IsInjected(Node? node)
        {
            return injectedNodes.Contains(node);
        }

        public void SetInjected(Node? node)
        {
            injectedNodes.Add(node);
        }
        
        

        public bool HasVariable(VSNodeBase owner, string name)
        {
            bool containsKey = NodeScriptNodeVariables.ContainsKey(owner);
            if (containsKey)
            {
                return NodeScriptNodeVariables[owner].ContainsKey(name) && NodeScriptNodeVariables[owner][name] != null;
            }
            return false;
        }
        
        public bool HasVariable(string name)
        {
            return NodeScriptExecutionVariables.ContainsKey(name) && NodeScriptExecutionVariables[name] != null;
        }
        
        public OperationResult<object> GetVariable(string name)
        {
            return NodeScriptExecutionVariables.TryGetValue(name, out object? value) ? 
                       OperationResult<object>.Success(value) : 
                       OperationResult<object>.Failure("no variable with name " + name);
        }
        public void SetVariable(string name, object? newValue)
        {
            NodeScriptExecutionVariables[name] = newValue;
        }
        

        public OperationResult<object> GetVariable(VSNodeBase owner, string name)
        {
            bool containsKey = NodeScriptNodeVariables.ContainsKey(owner);
            if (containsKey)
            {
                return NodeScriptNodeVariables[owner]!.TryGetValue(name, out object? value) ? 
                           OperationResult<object>.Success(value) : 
                           OperationResult<object>.Failure("no variable with name " + name);
            }
            return OperationResult<object>.Failure("Variable not found");
        }
        public void SetVariable(VSNodeBase owner, string name, object? newValue)
        {
            bool containsKey = NodeScriptNodeVariables.ContainsKey(owner);
            if (!containsKey)
            {
                NodeScriptNodeVariables.Add(owner, new ());
            }
            Dictionary<string,object?> ownerVariables = NodeScriptNodeVariables[owner];

            ownerVariables[name] = newValue;
            
        }
        public void RemoveVariables(VSNodeBase owner)
        {
            bool containsKey = NodeScriptNodeVariables.ContainsKey(owner);
            if (containsKey)
            {
                NodeScriptNodeVariables[owner].Clear();
            }
        }
    }
}