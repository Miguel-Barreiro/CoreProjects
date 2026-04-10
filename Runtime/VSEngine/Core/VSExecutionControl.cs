using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Core.Events;
using Core.Model;
using Core.Utils;
using Core.VSEngine.NestedVisualScripting;
using Core.Zenject.Source.Factories.Pooling.Static;
using Core.Zenject.Source.Internal;
using UnityEngine;
using UnityEngine.UIElements;
using VSEngine.Core.NestedVisualScripting;
using XNode;

#nullable enable

namespace Core.VSEngine
{
    
    public interface IVSExecutionControl
    {
        public void PushScript(ActionGraph actionGraph, ICacheOutputValues fromNode, Dictionary<string, object?> inputValues);
        public OperationResult<object> ResolveValue(string portName, Node origin);
        public OperationResult<object> ResolveMultiValue(string portName, int index, Node origin);
        public OperationResult<object> ResolveDynamicValue(string portName, Node origin);
        // public VSEventBase Event { get; }
        public IBaseEvent? CoreEvent { get; }
        public BaseEntityEvent? CoreEntityEvent { get; }
        public EntId OwnerId { get; }

        public List<T> GetCachedList<T>();
    }

    
    public class VSExecutionControl: IVSExecutionControl
    {
        private VSBaseEngine _vsBaseEngine;
        // private VSEventBase vsEvent;

        private BaseEvent? coreEvent;
        private BaseEntityEvent? coreEntityEvent;
        private EntId ownerId;

        public IBaseEvent CoreEvent => coreEvent != null? coreEvent : coreEntityEvent;
        public BaseEntityEvent? CoreEntityEvent => coreEntityEvent;
        public EntId OwnerId => ownerId;

        private readonly Stack<ScriptExecution> nestedScriptExecutionStack = new();
        
        public ExecutableNode? CurrentNode => CurrentScriptExecution?.CurrentNode; 
        public ScriptExecution? CurrentScriptExecution => nestedScriptExecutionStack.Count == 0? null : nestedScriptExecutionStack.Peek();
        
        // public VSEventBase Event => vsEvent;
        public List<T> GetCachedList<T>()
        {
            //TODO: implement pooling
            return new List<T>();
        }

        public void StartWith(VSBaseEngine vsBaseEngine, ExecutableNode startNode)
        {
            StartWith(vsBaseEngine, startNode, EntId.Invalid);
        }

        public void StartWith(VSBaseEngine vsBaseEngine, ExecutableNode startNode, EntId ownerId)
        {
            this._vsBaseEngine = vsBaseEngine;
            this.coreEvent = null;
            this.coreEntityEvent = null;
            this.ownerId = ownerId;

            nestedScriptExecutionStack.Clear();
            nestedScriptExecutionStack.Push(new ScriptExecution(startNode.graph as ActionGraph, startNode, null));
        }

        public void StartWithEvent(VSBaseEngine vsBaseEngine, BaseEvent vsEvent, ExecutableNode? startNode, EntId ownerId)
        {
            this._vsBaseEngine = vsBaseEngine;
            this.coreEvent = vsEvent;
            this.coreEntityEvent = null;
            this.ownerId = ownerId;

            nestedScriptExecutionStack.Clear();
            nestedScriptExecutionStack.Push(new ScriptExecution(startNode.graph as ActionGraph, startNode, null));
        }

        public void StartWithEntityEvent(VSBaseEngine vsBaseEngine, BaseEntityEvent vsEvent, ExecutableNode startNode, EntId ownerId)
        {
            this._vsBaseEngine = vsBaseEngine;
            this.coreEvent = null;
            this.coreEntityEvent = vsEvent;
            this.ownerId = ownerId;

            nestedScriptExecutionStack.Clear();
            nestedScriptExecutionStack.Push(new ScriptExecution(startNode.graph as ActionGraph, startNode, null));
        }

        
        // public void Start(VSEngineCore vsEngine, BaseEvent coreEvent, EntId ownerId, ExecutableNode? startNode)
        // {
        //     this.vsEngine = vsEngine;
        //     this.coreEvent = coreEvent;
        //     this.coreEntityEvent = null;
        //     this.ownerId = ownerId;
        //     // this.vsEvent = null!;
        //
        //     nestedScriptExecutionStack.Clear();
        //     nestedScriptExecutionStack.Push(new ScriptExecution(startNode.graph as ActionGraph, startNode, null));
        // }
        //
        // public void Start(VSEngineCore vsEngine, BaseEntityEvent coreEntityEvent, EntId ownerId, ExecutableNode? startNode)
        // {
        //     this.vsEngine = vsEngine;
        //     this.coreEntityEvent = coreEntityEvent;
        //     this.coreEvent = null;
        //     this.ownerId = ownerId;
        //     // this.vsEvent = null!;
        //
        //     nestedScriptExecutionStack.Clear();
        //     nestedScriptExecutionStack.Push(new ScriptExecution(startNode.graph as ActionGraph, startNode, null));
        // }
        

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
                _vsBaseEngine.InjectDependencies(currentNode, this);
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
                Debug.LogError($"No start node found in {actionGraph.name} for event {GetEventName()}");
                return;
            }

            ScriptExecution scriptExecution = new ScriptExecution(actionGraph, startNode, fromNode);
            nestedScriptExecutionStack.Push(scriptExecution);
            
            if (inputsNode != null)
            {
                foreach ((string inputName, object? value) in inputValues)
                    scriptExecution.SetGraphVariable( inputName, value);
            }
            
        }

        private string? GetEventName()
        {
            if(CoreEvent == null)
                return CoreEntityEvent?.GetType().Name;
            return CoreEvent.GetType().Name;
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


        public OperationResult<object> ResolveValue(string portName, Node origin)
        {
            NodePort? port = origin.GetPort(portName);

            if (port == null) {
                string message = $"Port {portName} for node {CurrentNode.name} in {origin.name} doesnt exist";
                Debug.LogError(message);
                return OperationResult<object>.Failure(message);
            }

            return ResolveValue(port, origin);
        }
        
        public OperationResult<object> ResolveMultiValue(string portName, int index, Node origin)
        {
            string portNameWithIndex = $"{portName} {index}";

            NodePort? port = origin.GetPort(portNameWithIndex);
            if (port == null) {
                // Log.LogError($"Port {portName} for node {CurrentNode?.name} in {origin.name} doesnt exist");
                return OperationResult<object>.Failure($"Port {portName} for node {CurrentNode?.name} in {origin.name} doesnt exist");
            }

            if (!port.IsInput)
            {
                string message = $"Resolve value called for non input Port {port.fieldName} in node {origin.name} in {origin.graph.name} ";
                Debug.LogError(message);
                return OperationResult<object>.Failure(message);
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
                    string message = $"node {origin.name} in {origin.graph.name} asked for invalid field(unconnected node) {port.fieldName}";
                    Debug.LogError(message);
                    return OperationResult<object>.Failure(message);
                }

                ICollection? elements = (ICollection)fieldInfo.GetValue(port.node);
                if(elements == null)
                {
                    return OperationResult<object>.Failure("field is not a collection or is null");
                }
                int idx = 0;
                foreach (object? element in elements)
                {
                    if(idx == index)
                    {
                        return OperationResult<object>.Success(element);
                    }
                    idx++;
                }
                return OperationResult<object>.Failure("out of range index for collection");
            }
        }
        
        public OperationResult<object> ResolveDynamicValue(string portName, Node origin)
        {
            NodePort? port = origin.GetPort(portName);

            if (port == null) {
                string message = $"Port {portName} for node {CurrentNode.name} in {origin.name} doesnt exist";
                Debug.LogError(message);
                return OperationResult<object>.Failure(message);
            }

            return ResolveValue(port, origin, true);
        }


        private OperationResult<object> ResolveValue(NodePort port, Node origin, bool isDynamic = false)
        {
       
            if (!port.IsInput)
            {
                string message = $"Resolve value called for non input Port {port.fieldName} in node {origin.name} in {origin.graph.name} ";
                Debug.LogError(message);
                return OperationResult<object>.Failure(message);
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
                    return OperationResult<object>.Failure("Dynamic port is not connected");

                Type type = origin.GetType();
                FieldInfo? fieldInfo = NodeUtils.GetFieldByName(type, port.fieldName);

                if (fieldInfo == null)
                {
                    string message = $"node {origin.name} in {origin.graph.name} asked for invalid field(unconnected node) {port.fieldName}";
                    Debug.LogError(message);
                    return OperationResult<object>.Failure(message);
                }
                
                return OperationResult<object>.Success(fieldInfo.GetValue(origin));
            }
        }
        
        private OperationResult<object> ResolveConnectedPort(NodePort port, Node origin)
        {
            Node? connectedNode = port.Connection.node;
            if (connectedNode == null)
            {
                string message = $"Port {port.fieldName} for node {origin.name} in {origin.graph.name} is not connected";
                Debug.LogError(message);
                return OperationResult<object>.Failure(message);
            }
            if (connectedNode is IValueNode valueNode)
            {
                InjectNode(connectedNode);
                return valueNode.GetValue(port.Connection.fieldName);
            }
            else
            {
                string message = $"node {connectedNode.name} in {origin?.graph.name} is not a value node";
                Debug.LogError(message);
                return OperationResult<object>.Failure(message);
            }
        }
        

        private void InjectNode(Node node)
        {
            ScriptExecution currentScriptExecution = nestedScriptExecutionStack.Peek();
            if (!currentScriptExecution.IsInjected(node))
            {
                _vsBaseEngine.InjectDependencies(node, this);
                currentScriptExecution.SetInjected(node);
            }
        }

        
        
        protected static readonly StaticMemoryPool<VSExecutionControl> Pool =
            new StaticMemoryPool<VSExecutionControl>(OnSpawned, OnDespawned);

        protected virtual void OnSpawned() { }
        protected virtual void OnDespawned() { }

        public static VSExecutionControl NEW() { return Pool.Spawn(); }

        private static void OnDespawned(VSExecutionControl obj) { (obj as VSExecutionControl)?.OnDespawned(); }

        private static void OnSpawned(VSExecutionControl? control)
        {
            if (control != null)
            {
                control.ownerId = EntId.Invalid;
                control.coreEntityEvent = null;
                control.coreEvent = null;
                control._vsBaseEngine = null;
            }
        }

        public void Dispose() { Pool.Despawn(this as VSExecutionControl); }


    }
}