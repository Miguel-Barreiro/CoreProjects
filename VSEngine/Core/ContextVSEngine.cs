using System;
using System.Collections.Generic;
using Core.VSEngine.Contexts;
using XNode;

namespace Core.VSEngine
{

    public class ContextVSEngine : VSEngineCore
    {
        
        private readonly List<ExecutionContext> staticContexts = new();
        public void AddContext(ExecutionContext executionContext)
        {
            if (!staticContexts.Contains(executionContext))
            {
                staticContexts.Add(executionContext);
            }
        }

        public void Run(NodeGraph nodeGraph, BaseEventListenNode eventListenNode, 
            VSEventBase vsEvent, List<RuntimeContexts> runtimeContexts = null)
        {
            VSContextExecutionControl executionControl = VSContextExecutionControl.New();

            if (runtimeContexts != null)
            {
                foreach (RuntimeContexts runtimeContext in runtimeContexts)
                {
                    executionControl.AddContext(runtimeContext);
                }
            }

            foreach (ExecutionContext staticContext in staticContexts)
            {
                executionControl.AddContext(staticContext);
            }
            
            RunInternal(nodeGraph, eventListenNode, vsEvent, executionControl);
        }

        public override void InjectDependencies(Node node, VSExecutionControl vsExecutionControl)
        {
            base.InjectDependencies(node, vsExecutionControl);

            if (vsExecutionControl is VSContextExecutionControl vsContextExecutionControl)
            {
                vsContextExecutionControl.InjectDependencies(node);
            }
        }


        private class VSContextExecutionControl : VSExecutionControl
        {
            private readonly Dictionary<Type, ExecutionContext> contexts = new();

            private VSContextExecutionControl() { }

            public static VSContextExecutionControl New()
            {
                return new VSContextExecutionControl();
            }

            internal void AddContext(ExecutionContext executionContext)
            {
                Type type = executionContext.GetType();
                Type contextNodeType = typeof(IContextNode<>).MakeGenericType(type);
                if (!contexts.ContainsKey(contextNodeType))
                {
                    contexts.Add(contextNodeType, executionContext);
                }
            }

            
            internal void InjectDependencies(Node node)
            {
                Type nodeType = node.GetType();

                foreach ((Type contextAwareType, ExecutionContext context) in contexts)
                {
                    if (contextAwareType.IsAssignableFrom(nodeType))
                    {
                        //here we are setting the right property Context of the node
                        contextAwareType.InvokeMember(nameof(IContextNode<ExecutionContext>.Context),
                            System.Reflection.BindingFlags.SetProperty,
                            null, node, new object[] {context});
                    }
                }
            }
        }
    }


}
