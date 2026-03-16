#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using Core.Events;
using Core.Utils;
using Core.Utils.CachedDataStructures;
using UnityEngine;
using XNode;

namespace Core.VSEngine
{
    public static class EventNodeUtils
    {

        internal static void FillFieldPorts(MemberInfo[] memberInfos, 
            Dictionary<string, VSFieldPort> result, bool isInput)
        {
            foreach (MemberInfo memberInfo in memberInfos)
            {
                VSFieldAttribute vsFieldAttribute = memberInfo.GetCustomAttribute<VSFieldAttribute>();
                if (vsFieldAttribute != null)
                {
                    if (result.ContainsKey(memberInfo.Name))
                    {
                        continue;
                    }
                
                    if (isInput && vsFieldAttribute.IsWritable)
                    {
                        result.Add(memberInfo.Name, new VSFieldPort()
                        {
                            isInput = isInput,
                            FieldName = memberInfo.Name, 
                            FieldType = memberInfo.GetUnderlyingType()
                        });
                        continue;
                    }
                    if (!isInput && vsFieldAttribute.IsReadable)
                    {
                        result.Add(memberInfo.Name, new VSFieldPort()
                        {
                            isInput = isInput,
                            FieldName = memberInfo.Name, 
                            FieldType = memberInfo.GetUnderlyingType()
                        });
                        continue;
                    }

                    // Debug.Log(
                    //     $"TEST: {memberInfo.Name} is writable: {vsEventAttribute.IsWritable} is readable: {vsEventAttribute.IsReadable}");
                }
            }
        }

        internal static void AddDynamicPorts(VSNodeBase node, List<VSFieldPort> fieldPorts)
        {
            foreach (VSFieldPort fieldPort in fieldPorts)
            {
                if (fieldPort.isInput && node.GetInputPort(fieldPort.FieldName) == null)
                {
                    node.AddDynamicInput(fieldPort.FieldType, fieldName: fieldPort.FieldName,
                        typeConstraint: Node.TypeConstraint.Strict, connectionType: Node.ConnectionType.Override);
                    continue;
                }
                if (!fieldPort.isInput && node.GetOutputPort(fieldPort.FieldName) == null)
                {
                    node.AddDynamicOutput(fieldPort.FieldType, fieldName: fieldPort.FieldName,
                        typeConstraint: Node.TypeConstraint.Strict, connectionType: Node.ConnectionType.Multiple);
                    continue;
                }
            }

            using CachedList<NodePort> portsToDelete = ListCache<NodePort>.Get();
            foreach (NodePort nodePort in node.Ports)
            {
                if (nodePort.IsDynamic)
                {
                    if (!IsFieldPort(nodePort, fieldPorts))
                    {
                        portsToDelete.Add(nodePort);
                    }
                }
            }
            foreach (NodePort nodePort in portsToDelete)
            {
                node.RemoveDynamicPort(nodePort);
            }
        }

        internal static void BuildFieldCache(Dictionary<string, VSFieldPort> result, List<VSFieldPort> vsFieldPorts)
        {
            foreach (VSFieldPort fieldPort in vsFieldPorts)
            {
                result.Add(fieldPort.FieldName, fieldPort);
            }
        }
        
        internal static void Write(VSNodeBase node, List<VSFieldPort> fieldPorts, IBaseEvent vsEvent, Type eventType)
        {
            
            foreach (VSFieldPort fieldPort in fieldPorts)
            {
                
                OperationResult<object> operationResult = node.ResolveDynamic(fieldPort.FieldName);

                if (operationResult.IsFailure)
                {
                    // Debug.LogWarning(operationResult.Exception.Message);
                    continue;
                }

                FieldInfo fieldInfo = eventType.GetField(fieldPort.FieldName);
                fieldInfo.SetValue(vsEvent, operationResult.Result);
            }
        }
        

        internal static bool IsFieldPort(NodePort nodePort, List<VSFieldPort> vsFieldPorts)
        {
            foreach (VSFieldPort fieldPort in vsFieldPorts)
            {
                if (nodePort.fieldName == fieldPort.FieldName && 
                    nodePort.ValueType == fieldPort.FieldType &&
                    nodePort.IsInput == fieldPort.isInput)
                {
                    return true;
                }
            }
            return false;
        }

        // Non-generic helpers for Core Event<T> / EntityEvent<T> types.
        // Scans all public instance fields for [VSField] — no VSEvent<T> wrapper assumption.
        internal static void CreateFieldPorts(Type eventType, List<VSFieldPort> vsFieldPorts, bool isInput)
        {
            FieldInfo[] fields = eventType.GetFields(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            Dictionary<string, VSFieldPort> fieldsByName = new();
            FillFieldPorts(fields, fieldsByName, isInput);
            vsFieldPorts.Clear();
            vsFieldPorts.AddRange(fieldsByName.Values);
        }

        // Reads a field value from a plain object using the cached FieldName on the port.
        internal static OperationResult<object> ReadFromObject(
            string portName, Dictionary<string, VSFieldPort> cache, object eventObj)
        {
            if (!cache.TryGetValue(portName, out VSFieldPort fieldPort))
                return OperationResult<object>.Failure($"No field cached for '{portName}'");

            FieldInfo? fieldInfo = eventObj.GetType().GetField(
                fieldPort.FieldName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            if (fieldInfo == null)
                return OperationResult<object>.Failure(
                    $"Field '{fieldPort.FieldName}' not found on {eventObj.GetType().Name}");

            return OperationResult<object>.Success(fieldInfo.GetValue(eventObj));
        }
    }

    public enum VSEventTiming { Pre, Default, Post }

    public enum VSEntityListenTarget
    {
        Owner,    // fires only when ev.EntityID == owner (the graph owner)
        Parent,   // not yet implemented — reserved
        All,      // fires for every entity's event
        Dynamic,  // fires when ev.EntityID matches the value of the "Target" input port
    }
        
    [Serializable]
    public class VSFieldPort
    {
        [SerializeField]
        public bool isInput;
        [SerializeField]
        public string FieldName;
        [SerializeField]
        public Type FieldType;
    }
}