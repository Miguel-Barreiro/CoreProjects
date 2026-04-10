using System.Collections.Generic;
using System.ComponentModel;
using Core.Events;
using Core.Utils;
using UnityEngine;
using XNode;

#nullable enable

namespace Core.VSEngine
{
    // [TypeConverter(typeof(SavableAssetSerializationConverter))]
    public abstract class VSNodeBase : Node
                                       // , IJsonSavableAsset
#if UNITY_EDITOR
        , ISerializationCallbackReceiver
#endif
    {
        public IVSExecutionControl ExecutionControl { protected get; set; }
        public ScriptExecution? ScriptExecution { protected get; set; }

        public string Guid => guid;
        [SerializeField, HideInInspector]
        private string guid;

        public long FileId => fileId;
        [SerializeField, HideInInspector]
        private long fileId;

        
#if UNITY_EDITOR
        protected virtual string[]? BuildNodeInfo()
        {
            return null;
        }
        
        private string[] nodeInfo = null;
        public string[] GetNodeInfo()
        {
            return nodeInfo;
        }
        public virtual void OnAfterDeserialize() { }
        
        public virtual void OnBeforeSerialize()
        {
            // this is to garantee that we update to the latest info every now and then 
            nodeInfo = BuildNodeInfo();
            FetchGUIDAndFileId();
        }
        
#endif

        protected OperationResult<T> INVALID_INPUT<T>(string portName)
        {
            return OperationResult<T>.Failure($"{portName} is not valid: in node {name} in graph {graph.name}");
        }

        protected OperationResult<T> INVALID_INPUT_NAME_CALLED<T>(string portName)
        {
            return OperationResult<T>.Failure($"{portName} given is not valid: in node {name} in graph {graph.name}");
        }

        
        protected OperationResult<T> INVALID_EXECUTION<T>(string message)
        {
            return OperationResult<T>.Failure($"invalid execution in node {name} in graph {graph.name} with message: {message}");
        }

        protected OperationResult<T> INVALID_EXECUTION_MESSAGE<T>(string message)
        {
            return OperationResult<T>.Failure($"{message} in node {name} in graph {graph.name} with message: {message}");
        }
        
        protected OperationResult<T> SUCCESS_RETURN<T>(T value)
        {
            return OperationResult<T>.Success(value);
        }
        

        // public virtual IJsonSavableAsset? GetChildAsset(long childFileId)
        // {
        //     return null;
        // }
        //
        // public IEnumerable<IJsonSavableAsset> GetChildAssets()
        // {
        //     yield break;
        // }

        public void FetchGUIDAndFileId()
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(GetInstanceID(), out guid, out fileId);
#endif
        }
        
        #region Local Variables

        protected void SetLocalVariable(string name, object? newValue)
        {
            ScriptExecution!.SetLocalVariable(this, name, newValue);
        }
        
        protected OperationResult<object?> GetLocalVariable(string name)
        {
            return ScriptExecution!.GetLocalVariable(this, name);
        }
        
        protected bool HasLocalVariable(string name)
        {
            return ScriptExecution!.HasLocalVariable(this, name);
        }
        
        protected void RemoveAllLocalVariables()
        {
            ScriptExecution!.RemoveAllLocalVariables(this);
        }
        
        #endregion
        
        
        
        protected bool Check(bool condition, string errorMessage)
        {
            if (condition)
                Debug.LogError($"{errorMessage} in {this.name} in graph { graph.name}");

            return condition;
        }
        
        // TODO: we need to add a better way to see if a value exists or not ( currently is not working for non nullable types)
        protected OperationResult<T> Resolve<T>(string portName)
        {
            OperationResult<object> resolvedValue = ExecutionControl.ResolveValue(portName, this);
            if(resolvedValue.IsFailure)
            {
                Debug.LogError($"Value resolved for port {portName} is null");
                return OperationResult<T>.Failure(resolvedValue.Exception);
            }
            else
            {
                return  OperationResult<T>.Success((T) resolvedValue.Result);
            }
        }

        // protected bool ResolveMultiValue<T>(string portName, int index, out T? result)
        // {
        //     OperationResult<object> operationResult = ExecutionControl.ResolveMultiValue(portName, index, this);
        //     if(operationResult.IsFailure)
        //     {
        //         result = (T)operationResult;
        //         return true;
        //     }
        //     else
        //     {
        //         result = default(T);
        //         return false;
        //     }
        // }
        
        public OperationResult<object> ResolveDynamic(string portName)
        {
            return ExecutionControl.ResolveDynamicValue(portName, this);
        }

        // protected OperationResult<List<T>> ResolveMultiValue<T>(string portName)
        // {
        //     T? currentValue;
        //     List<T> result = new List<T>();
        //     bool hasValue;
        //     
        //     do
        //     {
        //         hasValue = ResolveMultiValue<T>(portName, result.Count, out currentValue);
        //         if (hasValue && currentValue != null)
        //         {
        //             result.Add(currentValue);
        //         }
        //     }
        //     while (hasValue);
        //     return result;
        // }

        // protected bool ResolveMultiValue<T>(string portName, List<T> result)
        // {
        //     T? currentValue;
        //     bool hasValue;
        //     do
        //     {
        //         hasValue = ResolveMultiValue<T>(portName, result.Count, out currentValue);
        //         if (hasValue && currentValue != null)
        //         {
        //             result.Add(currentValue);
        //         }
        //     }
        //     while (hasValue);
        //     return result.Count > 0;
        // }

        
        #region Ports
        /// <summary> Returns output port which matches fieldName </summary>
        public NodePort? GetOutputPort(string fieldName) {
            NodePort port = GetPort(fieldName);
            if (port == null || port.direction != NodePort.IO.Output) return null;
            else return port;
        }

        /// <summary> Returns input port which matches fieldName </summary>
        public NodePort? GetInputPort(string fieldName) {
            NodePort port = GetPort(fieldName);
            if (port == null || port.direction != NodePort.IO.Input) return null;
            else return port;
        }

        /// <summary> Returns port which matches fieldName </summary>
        public NodePort? GetPort(string fieldName) {
            NodePort port;
            if (ports.TryGetValue(fieldName, out port)) return port;
            else return null;
        }


        public bool HasPort(string fieldName) {
            return ports.ContainsKey(fieldName);
        }
        #endregion
        
        protected Node? GetConnectedMultiNode(string portName, int index)
        {
            NodePort? exitPort = GetMultiNodePort(portName, index);
            
            if (exitPort == null)
            {
                return null;
            }

            return GetNodeFromPort(exitPort);
        }
        protected NodePort? GetMultiNodePort(string portName, int index)
        {
            return GetPort($"{portName} {index}");
        }
        
        // protected TEvent? GetEvent<TEvent>() where TEvent : VSEventBase
        // {
        //     return ExecutionControl.Event as TEvent;
        // }

        protected IBaseEvent? GetCoreEvent() => ExecutionControl.CoreEvent ?? ExecutionControl.CoreEntityEvent;

        protected BaseEntityEvent? GetCoreEntityEvent() => ExecutionControl.CoreEntityEvent;

        private static readonly Stack<ExecutableNode> utilReorderPorts = new Stack<ExecutableNode>();
        protected void MultipleContinue(string multiplePortName)
        {
            utilReorderPorts.Clear();
            
            for (int portNumber = 0; portNumber < 99; portNumber++)
            {
                NodePort? nodePort = GetMultiNodePort(multiplePortName, portNumber);
                if (nodePort == null)
                {
                    break;
                }

                NodePort? connection = nodePort.Connection;
                if (connection == null)
                {
                    continue;
                }

                Node? nextNode = connection.node;
                if (nextNode == null)
                {
                    continue;
                }
                
                if (nextNode is ExecutableNode executableNode)
                {
                    utilReorderPorts.Push(executableNode);
                }
                else
                {
                    Debug.LogError($"Node {name} in {graph.name} is not pointing to an executable node {nextNode.name}");
                }
            }

            while (utilReorderPorts.TryPop(out ExecutableNode nextNode))
            {
                ScriptExecution!.PushNode(nextNode);
            }
        }

        protected bool CanContinueWith(string portName)
        {
            Node? nextNode = GetNodeFromPort(portName);
            if (nextNode == null)
            {
                return false;
            }

            if (nextNode is ExecutableNode executableNode)
            {
                return true;
            }
            
            return false;
        }

        protected bool ContinueWith(string portName)
        {
            Node? nextNode = GetNodeFromPort(portName);
            if (nextNode == null)
            {
                // Log.LogError($"Node {name} in {graph.name} has no port named {portName}");
                return false;
            }

            if (nextNode is ExecutableNode executableNode)
            {
                ScriptExecution!.PushNode(executableNode);
                return true;
            }
            else
            {
                Debug.LogError($"Node {name} in {graph.name} is not pointing to an executable node {nextNode.name}");
            }
            return false;
        }
        
        public Node? GetNodeFromPort(string portName) {
            NodePort? port = GetOutputPort(portName);

            if (port == null)
            {
                Debug.LogError("port is null");

                return null;
            }

            return GetNodeFromPort(port);
        }




        /// <summary>
        /// This isn't used anywhere, it's probably just here to suppress xNode warnings.
        /// </summary>
        public override object? GetValue(NodePort port)
        {
            return null;
        }


    }
}