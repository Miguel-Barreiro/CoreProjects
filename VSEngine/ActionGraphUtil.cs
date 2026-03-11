using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using Core.Utils.CachedDataStructures;
using Core.VSEngine.SharedNodes;
using Core.VSEngine.SharedNodes.Scripting;
using XNode;

#nullable enable

namespace Core.VSEngine
{
    public static class ActionGraphUtil
    {
        /// <summary>
        /// Inject dependencies into subscripts recursively
        /// </summary>
        public static void InjectNodesRecursively(ActionGraph actionGraph, IEnumerable<IActionGraphContext> contexts, ActionGraphEnvironment actionGraphEnvironment)
        {
            foreach (Node? node in actionGraph.nodes)
            {
                if (node == null)
                {
                    continue;
                }

                //if (node is IInjectableCombatNode injectableCombatNode) {
                //    injectableCombatNode.Inject(engineGlobal, contexts, runningStatusEffectStateId, executionInstanceId);
                //}
                actionGraphEnvironment.InjectNode(actionGraph, node, contexts);

                if (node is ScriptNode scriptNode && scriptNode.Script != null) {
                    InjectNodesRecursively(scriptNode.Script, contexts, actionGraphEnvironment);
                }
            }
        }

        private static Node? PrepareGraphExecution(ActionGraph actionGraph,
            ActionGraphEnvironment actionGraphEnvironment,
            IEnumerable<IActionGraphContext>? contextsInput,
            Type? entryNodeType = null,
            Node? entryNode = null
            )
        {
            List<IActionGraphContext> contexts = new List<IActionGraphContext>();
            if (contextsInput != null)
            {
                contexts.AddRange(contextsInput);
            }

            // This was moved inside PrepareFirstExecution of the CombatActionGraphEnvironment
            /*
            // Inject new ExecutionInstanceContext if it doesn't exist and this graph is for a status effect
            if (contexts.GetContext<StatusEffectContext>().HasValue && executionInstanceId == null) {
                
                StatusEffectDb statusEffectDb = worldState.GetStatusEffectDb();
                executionInstanceId = statusEffectDb.CreateExecutionInstanceId();
            }
            */
            if (actionGraphEnvironment.FreshExecution)
            {
                actionGraphEnvironment.PrepareFirstExecution(contexts);
            }

            InjectNodesRecursively(actionGraph, contexts, actionGraphEnvironment);

            // Find entry node.
            Node? currentNode = entryNode;

            if (currentNode == null)
            {
                currentNode = actionGraphEnvironment.GetDefaultEntryNode(actionGraph, entryNodeType);
            }

            return currentNode;
        }

        /// <summary>
        /// Inject dependencies into a combat graph and execute it
        /// </summary>
        public static void ResolveGraph(
            ActionGraph actionGraph,
            ActionGraphEnvironment actionGraphEnvironment,
            IEnumerable<IActionGraphContext>? contextsInput,
            Type? entryNodeType = null,
            Node? entryNode = null
        ) {
            Node? currentNode = PrepareGraphExecution(actionGraph, actionGraphEnvironment, contextsInput, entryNodeType, entryNode);

            // Sometimes enterNode is null when we're checking for alternate entry node types
            // We don't need to emit an error here
            if (currentNode != null) {
                ResolveNodeChain(currentNode);
            }
        }

        public static async UniTask ResolveGraphAsync(
            ActionGraph actionGraph,
            ActionGraphEnvironment actionGraphEnvironment,
            IEnumerable<IActionGraphContext>? contextsInput = null,
            Type? entryNodeType = null,
            Node? entryNode = null
        )
        {
            Node? currentNode = PrepareGraphExecution(actionGraph, actionGraphEnvironment, contextsInput, entryNodeType, entryNode);

            // Sometimes enterNode is null when we're checking for alternate entry node types
            // We don't need to emit an error here
            if (currentNode != null)
            {
                await ResolveNodeChainAsync(currentNode);
            }
        }


        /// <summary>
        /// Execute a whole chain of nodes. Provides async resolvers for supported graphs.
        /// </summary>
        public static async UniTask ResolveNodeChainAsync(Node node) {
            using CachedList<Node> currentNodes = ListCache<Node>.Get();
            using CachedList<Node> nextNodes = ListCache<Node>.Get();
            nextNodes.Add(node);
            
            while (nextNodes.Count > 0)
            {
                currentNodes.Clear();
                currentNodes.AddRange(nextNodes);

                nextNodes.Clear();

                foreach (Node currentNode in currentNodes)
                {
                    if (currentNode is ISharedNode sharedNode)
                    {
                        await sharedNode.ResolveControlAsync(nextNodes);
                    }
                    else if(currentNode is IExecutable executableNode)
                    {
                        executableNode.ResolveControl(nextNodes);
                    }
                }
            }
        }
        
        /// <summary>
        /// Execute a whole chain of nodes.
        /// </summary>
        public static void ResolveNodeChain(Node node) {
            using CachedList<Node> currentNodes = ListCache<Node>.Get();
            using CachedList<Node> nextNodes = ListCache<Node>.Get();

            nextNodes.Add(node);
            
            while (nextNodes.Count > 0)
            {
                currentNodes.Clear();
                currentNodes.AddRange(nextNodes);

                nextNodes.Clear();

                foreach (Node currentNode in currentNodes)
                {
                    if (currentNode is ISharedNode sharedNode)
                    {
                        sharedNode.ResolveControl(nextNodes);
                    }
                    else if (currentNode is IExecutable executableNode)
                    {
                        executableNode.ResolveControl(nextNodes);
                    }
                }
            }
        }

        public static void ResolveNodeChain(NodePort port)
        {
            List<NodePort> connections = port.GetConnections();
            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i].node is Node node)
                {
                    ResolveNodeChain(node);
                    break;
                }
            }
        }

        /// <summary>
        /// A robust/intuitive/general purpose port value resolver that defaults to reflection
        /// </summary>
        public static object? ResolvePortValue(NodePort port) 
        {
            if (port.IsOutput)
            {
                if (port.node is ISharedNode sharedNode) {
                    return sharedNode.ResolveValue(port.fieldName);
                } else if (port.node is ActionNode actionNode) {
                    return actionNode.ResolveValue(port.fieldName);
                } else {
                    return port.GetOutputValue();
                }
            }

            if (port.node == null) {
                throw new Exception("port.node is null");
            }

            if (port.IsConnected)
            {
                NodePort connectedPort = port.GetConnection(0);
                Node? connectedNode = connectedPort.node;

                // This probably shouldn't happen but it's here if anyone wants to create ports this way
                if (connectedNode == null)
                {
                    return port.GetInputValue();
                }

                if (connectedNode is ISharedNode sharedNode) {
                    return sharedNode.ResolveValue(connectedPort.fieldName);
                } else if (connectedNode is ActionNode actionNode) {
                    return actionNode.ResolveValue(connectedPort.fieldName);
                } else {
                    return port.GetInputValue();
                }
            }
            else
            {
                // Sometimes ports aren't connected but still have a backing value to provide

                Type type = port.node.GetType();
                    
                FieldInfo? fieldInfo = type.GetField(
                    port.fieldName, 
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );

                return fieldInfo?.GetValue(port.node);
            }
        }

        public static object? ResolvePortValue(this Node node, NodePort port) 
        {
            return ResolvePortValue(port);
        }

        public static object? ResolvePortValue(this Node node, string portName) 
        {
            NodePort? port = node.GetPort(portName);

            if (port == null) {
                return null;
            }

            return ResolvePortValue(node, port);
        }

        public static T? GetContext<T>(this IEnumerable<IActionGraphContext> contexts) where T : struct, IActionGraphContext
        {
            foreach (IActionGraphContext context in contexts)
            {
                if (context is T typedContext)
                {
                    return typedContext;
                }
            }

            return default;
        }
        public static IActionGraphContext GetContext(this IEnumerable<IActionGraphContext> contexts, Type contextType)
        {
            foreach (IActionGraphContext context in contexts)
            {
                if (context.GetType() == contextType)
                {
                    return context;
                }
            }

            return default;
        }

       

    }
}