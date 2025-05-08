using System;
using System.Collections.Generic;
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

		private IEnumerable<(object, Type)> CreateAllComponentContainers(uint maxNumber)
		{
			IEnumerable<Type> allComponentDataTypes = TypeCache.Get().GetAllComponentDataTypes();
			
			foreach (var componentDataType in allComponentDataTypes)
			{
				var containerType = typeof(ComponentContainerImplementation<>).MakeGenericType(componentDataType);
				object newComponentContainer = Activator.CreateInstance(containerType, maxNumber);
				yield return (newComponentContainer, componentDataType);
			}
		}
		
		#endregion

	}
	
	
}