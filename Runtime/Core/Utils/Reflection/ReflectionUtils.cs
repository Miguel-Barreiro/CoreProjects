using System;
using System.Collections.Generic;
using System.Reflection;
using Core.Utils.CachedDataStructures;

namespace Core.Utils.Reflection
{
    public static class ReflectionUtils
    {
        public static IEnumerable<Type> GetImplementedInterfaces(this Type t)
        {
            using CachedHashset<Type> allInterfaces = HashsetCache<Type>.Get();

            GetInterfacesInternal(t, allInterfaces);
            foreach (Type currentInterface in allInterfaces)
            {
                yield return currentInterface;
            }
            
            static void GetInterfacesInternal(Type t, CachedHashset<Type> allInterfaces)
            {
                Type[] currentInterfaces = t.GetInterfaces();
                foreach (Type current in currentInterfaces)
                {
                    allInterfaces.Add(current);
                }
                if (t.BaseType != null)
                {
                    GetInterfacesInternal(t.BaseType, allInterfaces);
                }
            }
        }
        
        public static bool IsTypeOf<T>(this Type type)
        {
            return typeof (T).IsAssignableFrom(type);
        }
        public static bool IsTypeOf(this Type type, Type targetType)
        {
            return targetType.IsAssignableFrom(type);
        }

        public static bool IsAssignableToGenericWithArgType(this Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            Type baseType = givenType.BaseType;
            if (baseType == null) return false;

            return IsAssignableToGenericWithArgType(baseType, genericType);
        }
        
        
        
        public static bool ImplementsGenericFullTypeDefinition(this Type givenType, Type genericFullType)
        {
            if (givenType.IsGenericType && givenType.IsTypeOf(genericFullType))
            {
                return true;
            }
            
            var interfaceTypes = givenType.GetInterfaces();
            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && givenType.IsTypeOf(genericFullType))
                {
                    return true;
                }
            }

            Type parentType = givenType.BaseType;
            if (parentType == null) return false;

            return ImplementsGenericFullTypeDefinition(parentType, genericFullType);
        }
        
        
        
        // public static Type GetFirstGenericArgumentTypeDefinition(this Type givenType, Type genericType)
        // {
        //     if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
        //     {
        //         Type[] genericArguments = givenType.GetGenericArguments();
        //         if (genericArguments.Length > 0)
        //             return genericArguments[0];
        //         else
        //             // If the generic type has no arguments, return null
        //             return null;
        //     }
        //
        //
        //     var interfaceTypes = givenType.GetInterfaces();
        //     foreach (var it in interfaceTypes)
        //     {
        //         if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
        //         {
        //             Type[] genericArguments = it.GetGenericArguments();
        //             if (genericArguments.Length > 0)
        //                 return genericArguments[0];
        //         }
        //     }
        //
        //     Type parentType = givenType.BaseType;
        //     if (parentType == null) return null;
        //     
        //     return GetFirstGenericArgumentTypeDefinition(parentType, genericType);
        // }
        

        public static IEnumerable<Type> GetAllTypesOf<TTargetType>()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.IsTypeOf<TTargetType>() && type != typeof(TTargetType))
                    {
                        yield return type;
                    }
                }
            }
        }

        public static IEnumerable<Type> GetAllTypesImplementingGenericDefinition(Type genericTypeDefinition)
        {
            using CachedList<Type> result = ListCache<Type>.Get();
            
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    Type[] interfaces = type.GetInterfaces();
                    foreach (Type interfaceType in interfaces)
                    {
                        if(interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == genericTypeDefinition)
                        {
                            if (!result.Contains(type))
                                result.Add(type);
                        }
                    }
                }
            }
            foreach (Type type in result)
            {
                yield return type;
            }
        }

        
        
        public static IEnumerable<Type> GetAllTypesImplementingGeneric(Type genericType)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.IsAssignableToGenericWithArgType(genericType) && type != genericType)
                    {
                        yield return type;
                    }
                }
            }
        }
        
        
        /// <summary>
        /// Search for a method by name and parameter types.  
        /// Unlike GetMethod(), does 'loose' matching on generic
        /// parameter types, and searches base interfaces.
        /// </summary>
        /// <exception cref="AmbiguousMatchException"/>
        public static MethodInfo GetMethodExt(  this Type thisType, 
                                                string name, 
                                                params Type[] parameterTypes)
        {
            return GetMethodExt(thisType, 
                                name, 
                                BindingFlags.Instance 
                                | BindingFlags.Static 
                                | BindingFlags.Public 
                                | BindingFlags.NonPublic
                                | BindingFlags.FlattenHierarchy, 
                                parameterTypes);
        }

        /// <summary>
        /// Search for a method by name, parameter types, and binding flags.  
        /// Unlike GetMethod(), does 'loose' matching on generic
        /// parameter types, and searches base interfaces.
        /// </summary>
        /// <exception cref="AmbiguousMatchException"/>
        public static MethodInfo GetMethodExt(  this Type thisType, 
                                                string name, 
                                                BindingFlags bindingFlags, 
                                                params Type[] parameterTypes)
        {
            MethodInfo matchingMethod = null;

            // Check all methods with the specified name, including in base classes
            GetMethodExt(ref matchingMethod, thisType, name, bindingFlags, parameterTypes);

            // If we're searching an interface, we have to manually search base interfaces
            if (matchingMethod == null && thisType.IsInterface)
            {
                foreach (Type interfaceType in thisType.GetInterfaces())
                    GetMethodExt(ref matchingMethod, 
                                 interfaceType, 
                                 name, 
                                 bindingFlags, 
                                 parameterTypes);
            }

            return matchingMethod;
        }

        private static void GetMethodExt(   ref MethodInfo matchingMethod, 
                                            Type type, 
                                            string name, 
                                            BindingFlags bindingFlags, 
                                            params Type[] parameterTypes)
        {
            // Check all methods with the specified name, including in base classes
            MemberInfo[] memberInfos = type.GetMember(name, MemberTypes.Method,  BindingFlags.Instance | bindingFlags);
            foreach ( MemberInfo memberInfo in memberInfos)
            {
                MethodInfo methodInfo = memberInfo as MethodInfo;
                if (methodInfo == null)
                    continue;

                // Check that the parameter counts and types match, 
                // with 'loose' matching on generic parameters
                ParameterInfo[] parameterInfos = methodInfo.GetParameters();
                if (parameterInfos.Length == parameterTypes.Length)
                {
                    int i = 0;
                    for (; i < parameterInfos.Length; ++i)
                    {
                        if (!parameterInfos[i].ParameterType
                                              .IsSimilarType(parameterTypes[i]))
                            break;
                    }
                    if (i == parameterInfos.Length)
                    {
                        if (matchingMethod == null)
                            matchingMethod = methodInfo;
                        else
                            throw new AmbiguousMatchException(
                                   "More than one matching method found!");
                    }
                }
            }
        }

        /// <summary>
        /// Special type used to match any generic parameter type in GetMethodExt().
        /// </summary>
        private class T { }

        /// <summary>
        /// Determines if the two types are either identical, or are both generic 
        /// parameters or generic types with generic parameters in the same
        ///  locations (generic parameters match any other generic paramter,
        /// but NOT concrete types).
        /// </summary>
        private static bool IsSimilarType(this Type thisType, Type type)
        {
            // Ignore any 'ref' types
            if (thisType.IsByRef)
                thisType = thisType.GetElementType();
            if (type.IsByRef)
                type = type.GetElementType();

            // Handle array types
            if (thisType.IsArray && type.IsArray)
                return thisType.GetElementType().IsSimilarType(type.GetElementType());

            // If the types are identical, or they're both generic parameters 
            // or the special 'T' type, treat as a match
            if (thisType == type || ((thisType.IsGenericParameter || thisType == typeof(T)) 
                                 && (type.IsGenericParameter || type == typeof(T))))
                return true;

            // Handle any generic arguments
            if (thisType.IsGenericType && type.IsGenericType)
            {
                Type[] thisArguments = thisType.GetGenericArguments();
                Type[] arguments = type.GetGenericArguments();
                if (thisArguments.Length == arguments.Length)
                {
                    for (int i = 0; i < thisArguments.Length; ++i)
                    {
                        if (!thisArguments[i].IsSimilarType(arguments[i]))
                            return false;
                    }
                    return true;
                }
            }

            return false;
        }

    }
}