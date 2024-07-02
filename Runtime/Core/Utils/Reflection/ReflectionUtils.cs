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
                HashSet<Type> tt;
                
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
    }
}