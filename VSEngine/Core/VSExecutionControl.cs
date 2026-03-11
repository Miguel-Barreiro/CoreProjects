using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Core.VSEngine.NestedVisualScripting;
using Core.Zenject.Source.Internal;
using UnityEngine;
using VSEngine.Core.NestedVisualScripting;
using XNode;

#nullable enable

namespace Core.VSEngine
{
    
    public interface IVSExecutionControl
    {
        public void PushScript(ActionGraph actionGraph, ICacheOutputValues fromNode, Dictionary<string, object?> inputValues);
        public object? ResolveValue(string portName, Node origin);
        public object? ResolveMultiValue(string portName, int index, Node origin);
        public object? ResolveDynamicValue(string portName, Node origin);
        public VSEventBase Event { get; }

        public List<T> GetCachedList<T>();
    }

    
    public class VSExecutionControl: IVSExecutionControl
    {
        private VSEngineCore vsEngine;
        private VSEventBase vsEvent;

        private readonly Stack<ScriptExecution> nestedScriptExecutionStack = new();
        
        public ExecutableNode? CurrentNode => CurrentScriptExecution?.CurrentNode; 
        public ScriptExecution? CurrentScriptExecution => nestedScriptExecutionStack.Count == 0? null : nestedScriptExecutionStack.Peek();
        
        public VSEventBase Event => vsEvent;
        public List<T> GetCachedList<T>()
        {
            //TODO: implement pooling
            return new List<T>();
        }


        public void Start(VSEngineCore vsEngine, VSEventBase vsEvent, ExecutableNode? startNode)
        {
            this.vsEngine = vsEngine;
            this.vsEvent = vsEvent;
            
            nestedScriptExecutionStack.Clear();
            
            nestedScriptExecutionStack.Push(new ScriptExecution(startNode.graph as ActionGraph, startNode, null));
        }
        

        public void ExecuteCurrentNode()
        {
            ScriptExecution currentScriptExecution = nestedScriptExecutionStack.Peek();
            if (currentScriptExecution == null)
            {
                // we reached the end of the execution
                return;
            }

            ExecutableNode currentNode = currentScriptExecution.CurrentNode!;
            // Log.LogMessage($"Executing {currentNode.name}", LogPriority.Low);

            if (!currentScriptExecution.IsInjected(currentNode))
            {
                vsEngine.InjectDependencies(currentNode, this);
                currentScriptExecution.SetInjected(currentNode);
            }

            currentNode.Execute();
            currentScriptExecution.PopNode();

            if (nestedScriptExecutionStack.Count == 0)
            {
                // we reached the end of the execution
                return;
            }
            
            ScriptExecution newScriptExecution = nestedScriptExecutionStack.Peek();
            
            if (newScriptExecution.CurrentNode == null)
            {
                // Log.LogMessage($"Pop visual script {(newScriptExecution.Graph != null ? newScriptExecution.Graph.name : "(without graph)")}", LogPriority.Low);
                PopScript();
            }
        }
        
        public void PushScript(ActionGraph actionGraph, ICacheOutputValues fromNode, Dictionary<string, object?> inputValues)
        {
            InputVSNode? inputsNode = actionGraph.InputsNode;
            ExecutableNode? startNode = actionGraph.StartNode;

            if (startNode == null)
            {
                Debug.LogError($"No start node found in {actionGraph.name} for event {Event.GetType().Name}");
                return;
            }

            ScriptExecution scriptExecution = new ScriptExecution(actionGraph, startNode, fromNode);
            nestedScriptExecutionStack.Push(scriptExecution);
            
            if (inputsNode != null)
            {
                foreach ((string inputName, object? value) in inputValues)
                {
                    scriptExecution.SetVariable(inputsNode, inputName, value);
                }
            }
            
        }
        
        private void PopScript()
        {
            if (nestedScriptExecutionStack.Count == 0)
            {
                return;
            }
            ScriptExecution scriptExecution = nestedScriptExecutionStack.Peek();
            
            OutputVSNode? outputsNode = scriptExecution.Graph.OutputsNode;
            if (scriptExecution.FromNode != null && outputsNode != null)
            {
                InjectNode(outputsNode);
                Dictionary<string,object?> outputs = new Dictionary<string, object?>();
                outputsNode.SetOutputValues(outputs);
                scriptExecution.FromNode.CacheOutputValues(outputs);
            }
            
            nestedScriptExecutionStack.Pop();
        }


        public object? ResolveValue(string portName, Node origin)
        {
            NodePort? port = origin.GetPort(portName);

            if (port == null) {
                Debug.LogError($"Port {portName} for node {CurrentNode.name} in {origin.name} doesnt exist");
                return null;
            }

            return ResolveValue(port, origin);
        }
        
        public object? ResolveMultiValue(string portName, int index, Node origin)
        {
            string portNameWithIndex = $"{portName} {index}";

            NodePort? port = origin.GetPort(portNameWithIndex);
            if (port == null) {
                // Log.LogError($"Port {portName} for node {CurrentNode?.name} in {origin.name} doesnt exist");
                return null;
            }

            if (!port.IsInput)
            {
                Debug.LogError($"Resolve value called for non input Port {port.fieldName} in node {origin.name} in {origin.graph.name} ");
                return null;
            }

            //if we are getting a value from an input port that is not connected
            if (port.IsConnected && port.Connection != null)
            {
                return ResolveConnectedPort(port, origin);
            }
            else
            {
                Type type = origin.GetType();
                FieldInfo? fieldInfo = NodeUtils.GetFieldByName(type, portName);

                if (fieldInfo == null)
                {
                    Debug.LogError($"node {origin.name} in {origin.graph.name} asked for invalid field(unconnected node) {port.fieldName}");
                    return null;
                }

                ICollection? elements = (ICollection)fieldInfo.GetValue(port.node);
                if(elements == null)
                {
                    return null;
                }
                int idx = 0;
                foreach (object? element in elements)
                {
                    if(idx == index)
                    {
                        return element;
                    }
                    idx++;
                }
                return null;
            }
        }
        
        public object? ResolveDynamicValue(string portName, Node origin)
        {
            NodePort? port = origin.GetPort(portName);

            if (port == null) {
                Debug.LogError($"Port {portName} for node {CurrentNode.name} in {origin.name} doesnt exist");
                return null;
            }

            return ResolveValue(port, origin, true);
        }


        private object? ResolveValue(NodePort port, Node origin, bool isDynamic = false)
        {
       
            if (!port.IsInput)
            {
                Debug.LogError($"Resolve value called for non input Port {port.fieldName} in node {origin.name} in {origin.graph.name} ");
                return null;
            }
        
            //if we are getting a value from an input port that is not connected
            if (port.IsConnected && port.Connection != null)
            {
                return ResolveConnectedPort(port, origin);
            }
            else 
            {
                // if it is a dynamic port there is no field associated with it
                if (isDynamic)
                {
                    return null;
                }

                Type type = origin.GetType();
                FieldInfo? fieldInfo = NodeUtils.GetFieldByName(type, port.fieldName);

                if (fieldInfo == null)
                {
                    Debug.LogError($"node {origin.name} in {origin.graph.name} asked for invalid field(unconnected node) {port.fieldName}");
                    return null;
                }

                return fieldInfo?.GetValue(origin);
            }
        }
        
        private object? ResolveConnectedPort(NodePort port, Node origin)
        {
            Node? connectedNode = port.Connection.node;
            if (connectedNode == null)
            {
                Debug.LogError($"Port {port.fieldName} for node {origin.name} in {origin.graph.name} is not connected");
                return null;
            }
            if (connectedNode is IValueNode valueNode)
            {
                InjectNode(connectedNode);
                return valueNode.GetValue(port.Connection.fieldName);
            }
            else
            {
                Debug.LogError($"node {connectedNode.name} in {origin?.graph.name} is not a value node");
                return null;
            }
        }
        

        private void InjectNode(Node node)
        {
            ScriptExecution currentScriptExecution = nestedScriptExecutionStack.Peek();
            if (!currentScriptExecution.IsInjected(node))
            {
                vsEngine.InjectDependencies(node, this);
                currentScriptExecution.SetInjected(node);
            }
        }

    }
}