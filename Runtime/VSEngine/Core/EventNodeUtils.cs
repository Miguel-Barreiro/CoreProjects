#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using Core.Utils;
using Core.Utils.CachedDataStructures;
using UnityEngine;
using XNode;

namespace Core.VSEngine
{
    public static class EventNodeUtils
    {

        internal static void FillFieldPorts(MemberInfo[] memberInfos, 
            Dictionary<string, VSFieldPort> result, FieldOrigin origin, bool isInput)
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
                            origin = origin, 
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
                            origin = origin, 
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
        
        internal static void CreateFieldPorts<T>(List<VSFieldPort> vsFieldPorts, bool isInput)
            where T : VSEventBase
        {
            Type eventType = typeof(T);
            Type? eventDataType = GetEventDataType<T>();
            if (eventDataType == null)
            {
                Debug.LogError($"the event type given({typeof(T).Name}) is not an VSEvent ");
                return;
            }

            FieldInfo[] eventFields = eventType.GetFields();
            FieldInfo[] eventDataFields = eventDataType.GetFields();
            
            Dictionary<string, VSFieldPort> fieldsByName = new();
            FillFieldPorts(eventFields, fieldsByName, FieldOrigin.Event, isInput);
            FillFieldPorts(eventDataFields, fieldsByName, FieldOrigin.EventData, isInput);
            
            vsFieldPorts.Clear();
            vsFieldPorts.AddRange(fieldsByName.Values);
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
        
        internal static OperationResult<object> Read<T>(string fieldName, Dictionary<string, VSFieldPort> fieldPortsCache, T vsEvent)
            where T : VSEventBase
        {
            Type eventType = typeof(T);
            Type? eventDataType = GetEventDataType<T>();
            if (eventDataType == null)
            {
                string message = $"the event type given({typeof(T).Name}) is not an VSEvent ";
                Debug.LogWarning(message);
                return OperationResult<object>.Failure(message);
            }

            VSFieldPort fieldPort = fieldPortsCache[fieldName];
            
            if (fieldPort.origin == FieldOrigin.Event)
            {
                FieldInfo fieldInfo = eventType.GetField(fieldPort.FieldName);
                return OperationResult<object>.Success(fieldInfo.GetValue(vsEvent));
            }

            if (fieldPort.origin == FieldOrigin.EventData)
            {
                FieldInfo fieldInfo = eventDataType.GetField(fieldPort.FieldName);
                return OperationResult<object>.Success(fieldInfo.GetValue(vsEvent.EventData));
            }
            
            return OperationResult<object>.Failure("no field port found for " + fieldName);
        }
        
        internal static object? Read<T>(string fieldName, List<VSFieldPort> fieldPorts, T vsEvent)
            where T : VSEventBase
        {
            Type eventType = typeof(T);
            Type? eventDataType = GetEventDataType<T>();
            if (eventDataType == null)
            {
                Debug.LogWarning($"the event type given({typeof(T).Name}) is not an VSEvent ");
                return null;
            }

            
            foreach (VSFieldPort fieldPort in fieldPorts)
            {
                if (fieldPort.FieldName == fieldName)
                {
                    if (fieldPort.origin == FieldOrigin.Event)
                    {
                        FieldInfo fieldInfo = eventType.GetField(fieldPort.FieldName);
                        return fieldInfo.GetValue(vsEvent);
                    }

                    if (fieldPort.origin == FieldOrigin.EventData)
                    {
                        FieldInfo fieldInfo = eventDataType.GetField(fieldPort.FieldName);
                        return fieldInfo.GetValue(vsEvent.EventData);
                    }
                }
            }
            return null;   
        }
        
        internal static void Write<T>(VSNodeBase node, List<VSFieldPort> fieldPorts, T vsEvent)
            where T : VSEventBase
        {
            Type eventType = typeof(T);
            Type? eventDataType = GetEventDataType<T>();
            if (eventDataType == null)
            {
                Debug.LogWarning($"the event type given({typeof(T).Name}) is not an VSEvent ");
                return;
            }
            
            foreach (VSFieldPort fieldPort in fieldPorts)
            {
                object? fieldNewValue = node.ResolveDynamic(fieldPort.FieldName);

                if (fieldPort.origin == FieldOrigin.Event)
                {
                    FieldInfo fieldInfo = eventType.GetField(fieldPort.FieldName);
                    if (fieldNewValue != null)
                    {
                        fieldInfo.SetValue(vsEvent, fieldNewValue);
                    }
                }

                if (fieldPort.origin == FieldOrigin.EventData)
                {
                    FieldInfo fieldInfo = eventDataType.GetField(fieldPort.FieldName);
                    if (fieldNewValue != null)
                    {
                        fieldInfo.SetValue(vsEvent.EventData, fieldNewValue);
                    }
                }
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

        private static Type? GetEventDataType<T>()
            where T : VSEventBase
        {
            Type type = typeof(T);
            if (NodeUtils.IsOfGenericType( type, typeof(VSEvent<>), out var concreteType))
            {
                Type[] genericArguments = concreteType!.GetGenericArguments();
                return genericArguments[0];
            }

            return null;
        }
    }
    
    public enum FieldOrigin
    {
        Event, 
        EventData,
    }
        
    [Serializable]
    public class VSFieldPort
    {
        [SerializeField]
        public bool isInput;
        [SerializeField]
        public FieldOrigin origin;
        [SerializeField]
        public string FieldName;
        [SerializeField]
        public Type FieldType;
    }
}