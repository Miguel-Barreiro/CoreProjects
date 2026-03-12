#nullable enable

using System;
using System.Collections.Generic;
using Core.VSEngine.NestedVisualScripting;
using UnityEngine;
using XNode;

namespace Core.VSEngine
{
    public abstract class VSEngineCore
    {
        private static readonly int INFINITE_LOOP_CHECK = 999;
        
        protected virtual void RunInternal(NodeGraph nodeGraph, 
                                           BaseEventListenNode eventListenNode, 
            VSEventBase vsEvent, VSExecutionControl vsExecutionControl)
        {
            if (!vsEvent.IsPropagating)
            {
                Debug.Log($"Execution of {vsEvent.GetType().Name} for {eventListenNode.name} was canceled due because event isnt propagating ");
                return;
            }

            // Log.LogMessage($"VS: Start executing {vsEvent.GetType().Name} for {eventListenNode.name}", LogPriority.Low);
            vsExecutionControl.Start(this, vsEvent, eventListenNode);
            int stop = INFINITE_LOOP_CHECK;

            try
            {
                while (stop > 0 && vsExecutionControl.CurrentNode != null && vsEvent.IsPropagating)
                {
                    stop--;
                    vsExecutionControl.ExecuteCurrentNode();
                }
            }
            catch (Exception e)
            {
#if DEBUG_BUILD                
                string graphName = vsExecutionControl.CurrentScriptExecution.Graph.name;
                string currentNodeName = vsExecutionControl.CurrentNode.name;
                Log.LogError($"Exception while executing {vsEvent.GetType().Name} in <{graphName}>/<{currentNodeName}>:\n{e.Message}");
#endif
                throw;
            }

            // Log.LogMessage($"Finished executing {vsEvent.GetType().Name} for {eventListenNode.name}", LogPriority.Low);
        }

        public readonly struct Listener
        {
            public readonly BaseEventListenNode Node;
            public readonly Type EventType;

            public Listener(BaseEventListenNode node, Type eventType)
            {
                Node = node;
                EventType = eventType;
            }
        }

        public static void GetEventListenerNodes(NodeGraph nodeGraph, List<Listener> result)
        {
            foreach (Node node in nodeGraph.nodes)
            {
                if(node == null)
                {
                    Debug.LogWarning($"node is null in graph {nodeGraph.name}");
                    continue;
                }
                Type nodeType = node.GetType();
                bool isEventListenNode = nodeType.IsOfGenericType(typeof(EventListenNode<>), out Type concreteType);

                if (isEventListenNode)
                {
                    Type eventType = concreteType.GenericTypeArguments[0];
                    result.Add(new Listener(node as BaseEventListenNode, eventType));
                }
            }
        }
        
        public virtual void InjectDependencies(Node node, VSExecutionControl vsExecutionControl)
        {
            if (node is VSNodeBase vsNodeBase)
            {
                vsNodeBase.ExecutionControl = vsExecutionControl;
                vsNodeBase.ScriptExecution = vsExecutionControl.CurrentScriptExecution!;
            }
        }

        public static ExecutableNode? GetStartNode(ActionGraph actionGraph)
        {
            foreach (Node node in actionGraph.nodes)
            {
                if (node is StartVSNode executableNode)
                {
                    return executableNode;
                }
            }
            return null;
        }

        public static OutputVSNode? GetOutputNode(NodeGraph script)
        {
            foreach (Node node in script.nodes)
            {
                if (node is OutputVSNode outputsNode)
                {
                    return outputsNode;
                }
            }
            return null;
        }

        public static InputVSNode? GetInputNode(ActionGraph script)
        {
            foreach (Node node in script.nodes)
            {
                if (node is InputVSNode inputVSNode)
                {
                    return inputVSNode;
                }
            }
            return null;
        }
    }
}