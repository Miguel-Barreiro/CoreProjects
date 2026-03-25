using System.Collections.Generic;
using Core.VSEngine.Nodes.Events;

#nullable enable

namespace Core.VSEngine
{
    public static class ActionGraphUtil
    {

        public static void FindListenersFromGraph( ActionGraph graph, 
                                                   List<EventListenNode> resultEventListenNodes,
													List<EntityEventListenNode> resultEntityEventListenNodes )
        {

            foreach (var node in graph.nodes)
            {
                if (node is EventListenNode)
                    resultEventListenNodes.Add((EventListenNode)node);
                else if (node is EntityEventListenNode)
                    resultEntityEventListenNodes.Add((EntityEventListenNode)node);
            }
            
        }
        
        
        // /// <summary>
        // /// Inject dependencies into subscripts recursively
        // /// </summary>
        // public static void InjectNodesRecursively(ActionGraph actionGraph, ActionGraphEnvironment actionGraphEnvironment)
        // {
        //     foreach (Node? node in actionGraph.nodes)
        //     {
        //         if (node == null)
        //         {
        //             continue;
        //         }
        //
        //         //if (node is IInjectableCombatNode injectableCombatNode) {
        //         //    injectableCombatNode.Inject(engineGlobal, contexts, runningStatusEffectStateId, executionInstanceId);
        //         //}
        //         actionGraphEnvironment.InjectNode(actionGraph, node);
        //
        //         if (node is ScriptNode scriptNode && scriptNode.Script != null) {
        //             InjectNodesRecursively(scriptNode.Script, actionGraphEnvironment);
        //         }
        //     }
        // }
        //
        // private static Node? PrepareGraphExecution(ActionGraph actionGraph,
        //     ActionGraphEnvironment actionGraphEnvironment,
        //     Type? entryNodeType = null,
        //     Node? entryNode = null
        //     )
        // {
        //
        //     // This was moved inside PrepareFirstExecution of the CombatActionGraphEnvironment
        //     /*
        //     // Inject new ExecutionInstanceContext if it doesn't exist and this graph is for a status effect
        //     if (contexts.GetContext<StatusEffectContext>().HasValue && executionInstanceId == null) {
        //         
        //         StatusEffectDb statusEffectDb = worldState.GetStatusEffectDb();
        //         executionInstanceId = statusEffectDb.CreateExecutionInstanceId();
        //     }
        //     */
        //     if (actionGraphEnvironment.FreshExecution)
        //     {
        //         actionGraphEnvironment.PrepareFirstExecution();
        //     }
        //
        //     InjectNodesRecursively(actionGraph, actionGraphEnvironment);
        //
        //     // Find entry node.
        //     Node? currentNode = entryNode;
        //
        //     if (currentNode == null)
        //     {
        //         currentNode = actionGraphEnvironment.GetDefaultEntryNode(actionGraph, entryNodeType);
        //     }
        //
        //     return currentNode;
        // }

    }
}