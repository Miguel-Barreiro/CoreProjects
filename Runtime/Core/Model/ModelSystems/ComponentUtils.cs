using System;
using System.Collections.Generic;
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
				if(intrface.IsGenericType && intrface.IsAssignableToGenericType(typeof(IComponentSystem<>)))
				{
					return true;
				}
			}
			return false;
		}
		
		internal static IEnumerable<Type> GetAllComponentTypesFromSystem(Type systemType)
		{
			IEnumerable<Type> interfaces = systemType.GetImplementedInterfaces();
			foreach (Type intrface in interfaces)
			{
				if(intrface.IsGenericType && intrface.IsAssignableToGenericType(typeof(IComponentSystem<>)))
				{
					yield return intrface.GenericTypeArguments[0];
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