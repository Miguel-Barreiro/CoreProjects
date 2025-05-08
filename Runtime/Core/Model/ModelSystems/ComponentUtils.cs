using System;
using System.Collections.Generic;
using Core.Utils.CachedDataStructures;
using Core.Utils.Reflection;

namespace Core.Model.ModelSystems
{
	public static class ComponentUtils
	{
		
		internal static bool IsComponentSystem(Type systemType)
		{
			IEnumerable<Type> interfaces = systemType.GetImplementedInterfaces();
			foreach (Type intrface in interfaces)
			{
				if(intrface.IsGenericType && intrface.IsAssignableToGenericWithArgType(typeof(IComponentSystem<>)))
				{
					return true;
				}
			}
			return false;
		}
		
		internal static IEnumerable<Type> GetAllComponentDataTypesFromSystem(Type systemType)
		{
			using CachedList<Type> avoidingDuplicatedTypes = ListCache<Type>.Get();
			
			IEnumerable<Type> interfaces = systemType.GetImplementedInterfaces();
			foreach (Type intrface in interfaces)
			{
				if ( !intrface.IsGenericType || intrface.GenericTypeArguments.Length != 1) continue;

				Type componentTypeFromSystem = intrface.GenericTypeArguments[0];
	
				if(!avoidingDuplicatedTypes.Contains(componentTypeFromSystem) &&
					intrface.IsAssignableToGenericWithArgType(typeof(IComponentSystem<>)))
				{
					avoidingDuplicatedTypes.Add(componentTypeFromSystem);
					yield return componentTypeFromSystem;
				}
			}
		}

		internal static IEnumerable<Type> GetAllComponentTypesFromSystem(Type systemType)
		{
			using CachedList<Type> avoidingDuplicatedTypes = ListCache<Type>.Get();
			
			IEnumerable<Type> interfaces = systemType.GetImplementedInterfaces();
			foreach (Type intrface in interfaces)
			{
				if ( !intrface.IsGenericType || intrface.GenericTypeArguments.Length != 1) continue;
				
				if(!avoidingDuplicatedTypes.Contains(intrface) &&
					intrface.IsAssignableToGenericWithArgType(typeof(IComponentSystem<>)))
				{
					avoidingDuplicatedTypes.Add(intrface);
					yield return intrface;
				}
			}
		}

		
		
		public static Type GetComponentDataType(Type componentType)
		{
			Type componentDataType = TypeCache.Get().GetComponentDataTypeFromComponentType(componentType);
			return componentDataType;
		}
		
		
	}
	
	
	
	
}