using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Model.ModelSystems
{
	
	
	public interface DataContainersController
	{
		public void ResetContainer<T>(int maxNumber) where T : struct, IComponentData;
	}

	public class DataContainersControllerImplementation : DataContainersController
	{
		private readonly Dictionary<Type, object> ContainersByComponentType = new();
		
		private static DataContainersControllerImplementation? instance = null;

		internal static DataContainersControllerImplementation GetInstance()
		{
			if (instance == null)
			{
				instance = new DataContainersControllerImplementation();
			}
			
			return instance;
		}
		private DataContainersControllerImplementation()
		{
			foreach ((object container, Type type) in CreateAllComponentContainers(20))
			{
				ContainersByComponentType[type] = container;
			}
		}
		
		

		public void ResetContainer<T>(int maxNumber) 
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
		
		
		internal IEnumerable<(object container, Type componentType, Type containerType)> GetAllComponentContainers()
		{
			foreach ((Type componentType, object container)  in ContainersByComponentType)
			{
				var containerType = typeof(ComponentContainer<>).MakeGenericType(componentType);
				yield return (container, componentType, containerType);
			}
		}
		
		

		internal object GetComponentContainer(Type componentDataType)
		{
			if (!ContainersByComponentType.TryGetValue(componentDataType, out object container))
			{
				Debug.LogError($"No container found for component type {componentDataType}");
				return null;
			}
				
			return container;
		}

		private IEnumerable<(object, Type)> CreateAllComponentContainers(int maxNumber)
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