#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using Core.VSEngine.Nodes;
using XNode;

namespace Core.VSEngine
{
    public static class NodeUtils
    {
        public static bool IsOfGenericType(this Type typeToCheck, Type genericType)
        {
            Type concreteType;
            return typeToCheck.IsOfGenericType(genericType, out concreteType); 
        }

        public static bool IsOfGenericType(this Type typeToCheck, Type genericType, out Type concreteGenericType)
        {
            while (true)
            {
                concreteGenericType = null;

                if (genericType == null)
                    throw new ArgumentNullException(nameof(genericType));

                if (!genericType.IsGenericTypeDefinition)
                    throw new ArgumentException("The definition needs to be a GenericTypeDefinition", nameof(genericType));

                if (typeToCheck == null || typeToCheck == typeof(object))
                    return false;

                if (typeToCheck == genericType)
                {
                    concreteGenericType = typeToCheck;
                    return true;
                }

                if ((typeToCheck.IsGenericType ? typeToCheck.GetGenericTypeDefinition() : typeToCheck) == genericType)
                {
                    concreteGenericType = typeToCheck;
                    return true;
                }

                if (genericType.IsInterface)
                    foreach (var i in typeToCheck.GetInterfaces())
                        if (i.IsOfGenericType(genericType, out concreteGenericType))
                            return true;

                typeToCheck = typeToCheck.BaseType;
            }
        }
        
        public static Type GetUnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    throw new ArgumentException
                    (
                        "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
            }
        }


        public static Type GetListTypeFromElementType(Type listElementType)
        {
            Type listType = typeof(List<>).MakeGenericType(new [] { listElementType } );
            return listType;
        }
        
        
        
#if UNITY_EDITOR
        
        
        public static void CreateInputPortByType(VSNodeBase node, string portname, Type portType)
        {
            NodePort? valuePort = node.GetInputPort(portname);
            if (valuePort != null && valuePort.ValueType != portType)
            {
                node.RemoveDynamicPort(portname);
                valuePort = null;
            }
            if (valuePort == null)
            {
                node.AddDynamicInput(portType, fieldName: portname,
                    typeConstraint: Node.TypeConstraint.Strict, connectionType: Node.ConnectionType.Override);
            }
        }
        
        public static void CreateOutputPortByType(VSNodeBase node, string portname, Type portType)
        {
            NodePort? valuePort = node.GetOutputPort(portname);
            if (valuePort != null && valuePort.ValueType != portType)
            {
                node.RemoveDynamicPort(portname);
                valuePort = null;
            }
            if (valuePort == null)
            {
                node.AddDynamicOutput(portType, fieldName: portname,
                    typeConstraint: Node.TypeConstraint.Strict, connectionType: Node.ConnectionType.Multiple);
            }
        }
        
        public static void CreateInputPortByNodeType(VSNodeBase node, string portname, NodeElementType type)
        {
            Type portType = type.GetLogicType();
            
            NodePort? valuePort = node.GetInputPort(portname);
            if (valuePort != null && valuePort.ValueType != portType)
            {
                node.RemoveDynamicPort(portname);
                valuePort = null;
            }
            if (valuePort == null)
            {
                node.AddDynamicInput(portType, fieldName: portname,
                    typeConstraint: Node.TypeConstraint.Strict, connectionType: Node.ConnectionType.Multiple);
            }
        }
        public static void CreateOutputPortByNodeType(VSNodeBase node, string portname, NodeElementType type)
        {
            Type portType = type.GetLogicType();
            
            NodePort? valuePort = node.GetOutputPort(portname);
            if (valuePort != null && valuePort.ValueType != portType)
            {
                node.RemoveDynamicPort(portname);
                valuePort = null;
            }
            if (valuePort == null)
            {
                node.AddDynamicOutput(portType, fieldName: portname,
                    typeConstraint: Node.TypeConstraint.Strict, connectionType: Node.ConnectionType.Multiple);
            }
        }
        
#endif        
        

        public static FieldInfo? GetFieldByName(Type type, string fieldName)
        {
            FieldInfo? fieldInfo = type.GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly
            );
            if (fieldInfo != null)
            {
                return fieldInfo;
            }

            if (type.BaseType != null)
            {
                return GetFieldByName(type.BaseType, fieldName);
            }

            return null;
        }
    }
}