#nullable enable

using System;
using System.Collections.Generic;
using Core.Utils;
using VSEngine.Core.NestedVisualScripting;
using XNode;

namespace Core.VSEngine
{

    public interface IScriptExecution
    {
        
        /// <summary>
        /// local variables exist only inside each node they are useful for storing temporary values during the execution
        /// of a node, and will not affect other nodes. will be cleared when execution ends
        /// </summary>
        #region Local Variables

        public bool HasLocalVariable(VSNodeBase owner, string name);
        public void SetLocalVariable(VSNodeBase owner, string name, object? value);
        public OperationResult<object?> GetLocalVariable(VSNodeBase owner, string name);
        
        public void RemoveAllLocalVariables(VSNodeBase owner);

        #endregion
        
        /// <summary>
        /// Graph variables exist inside each graph execution node they are useful for storing temporary values
        /// during the execution that will be used by all nodes. will be cleared when execution ends
        /// </summary>
        #region Local Variables

        public bool HasGraphVariable(string name);
        public void SetGraphVariable(string name, object? value);
        public OperationResult<object?> GetGraphVariable(string name);

        #endregion
        
        
    }

    public sealed class ScriptExecution : IScriptExecution
    {
        private readonly HashSet<Node?> injectedNodes = new();

        //this is the node that pushed this script execution
        private ICacheOutputValues? fromNode;
        
        private readonly ActionGraph? nodeGraph;
        public ActionGraph? Graph => nodeGraph;
        public ICacheOutputValues? FromNode => fromNode;
        public ExecutableNode? CurrentNode { set; get; }

        private readonly Stack<ExecutableNode?> NodeExecutionStack = new();
        
        private readonly Dictionary<VSNodeBase, Dictionary<string, object?>> LocalNodeVariables = new();
        private readonly Dictionary<string, object?> GraphExecutionVariables = new();

        public ScriptExecution(ActionGraph nodeGraph, ExecutableNode? startNode, ICacheOutputValues? fromNode = null)
        {
            injectedNodes.Clear();
            NodeExecutionStack.Clear();
            LocalNodeVariables.Clear();
            
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
        
        
        
        #region LOCAL VARIABLES

        public bool HasLocalVariable(VSNodeBase owner, string name)
        {
            throw new NotImplementedException();
        }
        public void SetLocalVariable(VSNodeBase owner, string name, object? newValue)
        {
            bool containsKey = LocalNodeVariables.ContainsKey(owner);
            if (!containsKey)
            {
                //TODO: we need to remove this dictionary allocation everytime
                LocalNodeVariables.Add(owner, new ());
            }
            Dictionary<string,object?> ownerVariables = LocalNodeVariables[owner];

            ownerVariables[name] = newValue;

        }
        public OperationResult<object?> GetLocalVariable(VSNodeBase owner, string name)
        {
            bool containsKey = LocalNodeVariables.ContainsKey(owner);
            if (containsKey)
            {
                return LocalNodeVariables[owner]!.TryGetValue(name, out object? value) ? 
                           OperationResult<object?>.Success(value) : 
                           OperationResult<object?>.Failure("no variable with name " + name);
            }
            
            return OperationResult<object?>.Failure("Variable not found");
        }

        public void RemoveAllLocalVariables(VSNodeBase owner)
        {
            bool containsKey = LocalNodeVariables.ContainsKey(owner);
            if (containsKey)
            {
                LocalNodeVariables[owner].Clear();
            }
        }
        
        #endregion

        
        #region GRAPH VARIABLES
        
        public bool HasGraphVariable(string name)
        {
            return GraphExecutionVariables.ContainsKey(name);
        }
        public void SetGraphVariable(string name, object? newValue)
        {
            GraphExecutionVariables[name] = newValue;
        }
        public OperationResult<object?> GetGraphVariable(string name)
        {
            return GraphExecutionVariables.TryGetValue(name, out object? value) ? 
                       OperationResult<object?>.Success(value) : 
                       OperationResult<object?>.Failure($"no graph variable with name <{name}>");

        }
        
        
        #endregion
        
    }
    
    
}