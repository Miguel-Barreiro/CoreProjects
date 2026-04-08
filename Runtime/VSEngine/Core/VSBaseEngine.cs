#nullable enable

using System;
using System.Collections.Generic;
using Core.Events;
using Core.Initialization;
using Core.Model;
using Core.VSEngine.NestedVisualScripting;
using Core.VSEngine.Nodes.Events;
using Core.VSEngine.Nodes.TestNodes;
using UnityEngine;
using UnityEngine.UIElements;
using XNode;
using Zenject;

namespace Core.VSEngine
{
    
    public sealed class VSEngineCore : VSBaseEngine
    {
        // This class is intentionally left blank as a non-abstract entry point for the engine.
        // All core functionality is implemented in the base class, allowing for future extensions if needed.
    }
    
    
    public abstract class VSBaseEngine
    {
        private static readonly int INFINITE_LOOP_CHECK = 999;

        [Inject] private readonly ObjectBuilder ObjectBuilder = null!;
        
        public void RunTestNode(BaseTestAssertNode node)
            => RunInternalEvent(node.graph, node );
        
        public void RunTestEventForEntity(BaseTestAssertNode node, EntId ownerId)
            => RunInternalEvent(node.graph, node, ownerId );
        
        public void RunEvent(BaseEventListenNode node, BaseEvent ev, EntId ownerId)
            => RunInternalEvent(node.graph, node, ev, ownerId );

        public void RunEntityEvent(BaseEventListenNode node, BaseEntityEvent ev, EntId ownerId)
            => RunInternalEntityEvent(node.graph, node, ev, ownerId);



        protected virtual void RunInternalEvent(NodeGraph nodeGraph,
                                                BaseTestAssertNode assertNode)
        {
            VSExecutionControl vsExecutionControl = VSExecutionControl.NEW();
            vsExecutionControl.StartWith(this, assertNode);
            
            LoopWithoutEvent(vsExecutionControl);
        }

        protected virtual void RunInternalEvent(NodeGraph nodeGraph,
                                                BaseTestAssertNode assertNode,
                                                EntId ownerId)
        {
            VSExecutionControl vsExecutionControl = new VSExecutionControl();            
            vsExecutionControl.StartWith(this, assertNode, ownerId);
            
            LoopWithoutEvent(vsExecutionControl);
        }

        private static void LoopWithoutEvent(VSExecutionControl vsExecutionControl)
        {
            int stop = INFINITE_LOOP_CHECK;

            try
            {
                while (stop > 0 && vsExecutionControl.CurrentNode != null )
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
                Log.LogError($"Exception while executing test in <{graphName}>/<{currentNodeName}>:\n{e.Message}");
#endif
                throw;
            }
        }


        protected virtual void RunInternalEvent(NodeGraph nodeGraph,
                                                BaseEventListenNode eventListenNode,
                                                BaseEvent vsEvent, EntId ownerId)
        {
            VSExecutionControl vsExecutionControl = new VSExecutionControl();
            
            if (!vsEvent.IsPropagating)
            {
                Debug.Log($"Execution of {vsEvent.GetType().Name} for {eventListenNode.name} was canceled due because event isnt propagating ");
                return;
            }

            // Log.LogMessage($"VS: Start executing {vsEvent.GetType().Name} for {eventListenNode.name}", LogPriority.Low);
            vsExecutionControl.StartWithEvent(this, vsEvent, eventListenNode, ownerId);
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

        protected virtual void RunInternalEntityEvent(NodeGraph nodeGraph,
            BaseEventListenNode eventListenNode,
            BaseEntityEvent coreEvent,
            EntId ownerId)
        {
            VSExecutionControl vsExecutionControl = VSExecutionControl.NEW();
            vsExecutionControl.StartWithEntityEvent(this, coreEvent, eventListenNode, ownerId);
            int stop = INFINITE_LOOP_CHECK;
            while (stop > 0 && vsExecutionControl.CurrentNode != null)
            {
                stop--;
                vsExecutionControl.ExecuteCurrentNode();
            }
        }

        // protected virtual void RunInternal(NodeGraph nodeGraph,
        //     BaseEventListenNode eventListenNode,
        //     BaseEntityEvent coreEvent,
        //     EntId ownerId,
        //     VSExecutionControl vsExecutionControl)
        // {
        //     vsExecutionControl.Start(this, coreEvent, ownerId, eventListenNode);
        //     int stop = INFINITE_LOOP_CHECK;
        //     while (stop > 0 && vsExecutionControl.CurrentNode != null)
        //     {
        //         stop--;
        //         vsExecutionControl.ExecuteCurrentNode();
        //     }
        // }
        

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
                
                // New non-generic paths
                if (node is EventListenNode eln && eln.EventType != null)
                {
                    result.Add(new Listener(eln, eln.EventType));
                    continue;
                }
                if (node is EntityEventListenNode eeln && eeln.EventType != null)
                {
                    result.Add(new Listener(eeln, eeln.EventType));
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
            ObjectBuilder.Inject(node);
        }

        public static ExecutableNode? GetStartNode(ActionGraph actionGraph)
        {
            foreach (Node node in actionGraph.nodes)
            {
                if (node is StartVSNode executableNode)
                    return executableNode;

            }
            return null;
        }

        public static void GetAssertNodes(ActionGraph actionGraph, List<BaseTestAssertNode> result)
        {
            foreach (Node node in actionGraph.nodes)
                if (node is BaseTestAssertNode assertNode)
                    result.Add(assertNode);
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