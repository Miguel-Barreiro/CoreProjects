using System;
using System.Collections.Generic;
using Core.Utils;

namespace Core.VSEngine {
    /// <summary>
    /// Serializable struct for types, used primarily in subscripting
    /// </summary>
    [Serializable]
    public struct SerializedType 
    {
        public string TypeName;

        /// <summary>
        /// We need an assembly qualified type name in order to convert from <see cref="string"/> to <see cref="Type"/>
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
        public SerializedType SerializedType;
        public string ParameterName;

        public SerializedTypeParameter(SerializedType serializedType, string parameterName)
        {
            SerializedType = serializedType;
            ParameterName = parameterName;
        }
    }

    public static class SerializedTypeUtils {
        public static Type GetParsedType(this SerializedType serializedType)
        {
           return GetParsedType(serializedType.AssemblyQualifiedName);
        }

        public static Type GetParsedType(string assemblyQualifiedName)
        {
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
            return serializedType.TypeName.Trim().Length != 0;
        }

        public static bool IsValid(this SerializedTypeParameter parameter) {
            return parameter.SerializedType.IsValid() && parameter.ParameterName.Trim().Length != 0;
        }
    }
}