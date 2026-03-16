using System;
using System.Collections.Generic;
using Core.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.VSEngine {
    /// <summary>
    /// Serializable struct for types, used primarily in subscripting.
    /// Use the inspector dropdown to select a type — edit via <see cref="SerializedTypeDrawer"/>.
    /// </summary>
    [Serializable]
    public struct SerializedType
    {
        /// <summary>Display name of the selected type.</summary>
        public string TypeName;

        /// <summary>
        /// Assembly-qualified name used to resolve the <see cref="Type"/> at runtime.
        /// </summary>
        public string AssemblyQualifiedName;

        public SerializedType(Type type)
        {
            TypeName = type.Name;
            AssemblyQualifiedName = type.AssemblyQualifiedName;
        }
        
    }

    /// <summary>
    /// An extension of <see cref="SerializedType"/> that provides a name to map to a type
    /// </summary>
    [Serializable]
    public struct SerializedTypeParameter
    {
        [ShowInInspector] public Type Type;

        public SerializedType SerializedType;
        public string ParameterName;

        public SerializedTypeParameter(SerializedType serializedType, string parameterName, Type type)
        {
            SerializedType = serializedType;
            ParameterName = parameterName;
            Type = type;
        }
    }

    public static class SerializedTypeUtils {
        public static Type GetParsedType(this SerializedType serializedType)
        {
           return GetParsedType(serializedType.AssemblyQualifiedName);
        }

        public static Type GetParsedType(string assemblyQualifiedName)
        {
            if (string.IsNullOrEmpty(assemblyQualifiedName)) return null;
            return Type.GetType(assemblyQualifiedName);
        }

        public static string GeneratePrettyTypeName(Type type)
        {
            List<string> generics = new();

            foreach (Type generic in type.GetGenericArguments())
            {
                generics.Add(GeneratePrettyTypeName(generic));
            }

            string name = type.Name.ReplaceRegex(@"`+[0-9]+", "");

            return generics.Count > 0 ? $"{name}<{string.Join(", ", generics)}>" : name;
        }

        public static bool IsValid(this SerializedType serializedType) {
            return !string.IsNullOrWhiteSpace(serializedType.AssemblyQualifiedName);
        }

        public static bool IsValid(this SerializedTypeParameter parameter) {
            return parameter.SerializedType.IsValid() && parameter.ParameterName.Trim().Length != 0;
        }
    }
}