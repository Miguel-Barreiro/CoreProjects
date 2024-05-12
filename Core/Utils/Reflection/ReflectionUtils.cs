using System;
using System.Collections.Generic;
using Core.Utils.CachedDataStructures;
using Unity.VisualScripting;

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
                allInterfaces.AddRange(currentInterfaces);
                if (t.BaseType != null)
                {
                    GetInterfacesInternal(t.BaseType, allInterfaces);
                }
            }
        }
        
        
    }
}