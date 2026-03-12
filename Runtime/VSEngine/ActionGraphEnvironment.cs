using System;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using Zenject;

#nullable enable

namespace Core.VSEngine
{
    public abstract class ActionGraphEnvironment
    {
        public bool FreshExecution { get; private set; }

        public ActionGraphEnvironment()
        {
            FreshExecution = true;
        }

        /// <summary>
        /// Implement this method to inject desired dependencies into the nodes of the graph
        /// </summary>
        /// <param name="actionGraph"></param>
        /// <param name="node"></param>
        /// <param name="contexts"></param>
        public abstract void InjectNode(ActionGraph actionGraph, Node node);

        /// <summary>
        /// Implement this method to use a custom logic to find the entry node of a graph in case no specific
        /// node to execute is specified. The default implementation searches for the first node of the specifid class type
        /// and returns it (returns null if no node of that type are present).
        /// </summary>
        /// <param name="actionGraph"></param>
        /// <returns></returns>
        public virtual Node? GetDefaultEntryNode(ActionGraph actionGraph, Type? entryNodeType)
        {
            foreach (Node graphNode in actionGraph.nodes)
            {
                if (entryNodeType == null || entryNodeType.IsAssignableFrom(graphNode.GetType()))
                {
                    return graphNode;
                }   
            }
            return null;
        }

        /// <summary>
        /// Implement this methods to add custom contexts for your nodes that is used only for this specific environment or to
        /// do context based computations for initializing this environment
        /// Default implementation stores the info that the execution is fresh no more (a previous execution has been done)
        /// The method will be called only once at the first execution just after 
        /// </summary>
        /// <param name="contexts"></param>
        public virtual void PrepareFirstExecution()
        {
            FreshExecution = false;
        }

    }
}
