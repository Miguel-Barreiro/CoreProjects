using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Model
{
	
	public interface IComponentData
	{
		public EntId ID { get; set; }
	}

	public interface Component<T> where T : IComponentData
	{
		public EntId ID { get; set; }
	}

	
	
	
	public static class ComponentContainersController
	{
		private static readonly Dictionary<Type, object> ContainersByComponentType = new();
		
		public static void ResetContainer<T>(int maxNumber) 
			where T : struct, IComponentData
		{
			Type componentType = typeof(T);
			if (!ContainersByComponentType.TryGetValue(componentType, out object container))
			{
				Debug.LogError($"No container found for component type {componentType}");
				return;
			}
				
			((ComponentContainerImplementation<T>)container).RebuildWithMax(maxNumber);
		}

		


		#region Internal

		internal static IEnumerable<Type> GetAllComponentDataTypes()
		{
			var componentTypes = AppDomain.CurrentDomain.GetAssemblies()
										.SelectMany(assembly => assembly.GetTypes())
										.Where(type => type.IsValueType && 
														type.GetInterfaces().Contains(typeof(IComponentData)));

			foreach (var type in componentTypes)
			{
				yield return type;
			}
		}
		
		
		internal static IEnumerable<(object container, Type componentType, Type containerType)> GetAllComponentContainers()
		{
			foreach ((Type componentType, object container)  in ContainersByComponentType)
			{
				var containerType = typeof(ComponentContainer<>).MakeGenericType(componentType);
				yield return (container, componentType, containerType);
			}
		}
		

		
		
		static ComponentContainersController()
		{
			foreach ((object container, Type type)  in CreateAllComponentContainers(20))
			{
				ContainersByComponentType[type] = container;
			}
		}

		internal static object GetComponentContainer(Type componentDataType)
		{
			if (!ContainersByComponentType.TryGetValue(componentDataType, out object container))
			{
				Debug.LogError($"No container found for component type {componentDataType}");
				return null;
			}
				
			return container;
		}

		private static IEnumerable<(object, Type)> CreateAllComponentContainers(int maxNumber)
		{
			IEnumerable<Type> allComponentDataTypes = GetAllComponentDataTypes();
			
			foreach (var type in allComponentDataTypes)
			{
				var containerType = typeof(ComponentContainerImplementation<>).MakeGenericType(type);
				object newComponentContainer = Activator.CreateInstance(containerType, maxNumber);
				yield return (newComponentContainer, type);
			}
		}
		
		#endregion

	}
	
	
}