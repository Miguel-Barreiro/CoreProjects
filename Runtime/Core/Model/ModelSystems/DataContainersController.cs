using System;
using System.Collections.Generic;
using Core.Systems;
using Core.Utils.Reflection;
using UnityEngine;

namespace Core.Model.ModelSystems
{
	
	
	public interface DataContainersController
	{
		public void ResizeComponentsContainer<T>(uint maxNumber) where T : struct, IComponentData;
	}

	public class DataContainersControllerImplementation : DataContainersController
	{
		private readonly Dictionary<Type, object> ContainersByComponentDataType = new();
		
		private static DataContainersControllerImplementation? instance = null;

		public static DataContainersControllerImplementation GetInstance()
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
				ContainersByComponentDataType[type] = container;
			}
		}
		
		

		public void ResizeComponentsContainer<T>(uint maxNumber) 
			where T : struct, IComponentData
		{
			Type componentDataType = typeof(T);
			if (!ContainersByComponentDataType.TryGetValue(componentDataType, out object container))
			{
				Debug.LogError($"No container found for component type {componentDataType}");
				return;
			}
			((IGenericComponentContainer)container).RebuildWithMax(maxNumber);
		}

		


		#region Internal

		public IEnumerable<(object container, Type componentType, Type containerType)> GetAllComponentContainers()
		{
			foreach ((Type componentDataType, object container) in ContainersByComponentDataType)
			{
				Type customContainerType = GetCustomContainerType(componentDataType);
				// var containerType = typeof(ComponentContainer<>).MakeGenericType(componentDataType);
				yield return (container, componentDataType, customContainerType);
			}
		}
		
		
		
		
		internal object GetComponentContainer(Type componentDataType)
		{
			if (!ContainersByComponentDataType.TryGetValue(componentDataType, out object container))
			{
				Debug.LogError($"No container found for component type {componentDataType}");
				return null;
			}
				
			return container;
		}

		private Type GetCustomContainerType(Type componentDataType)
		{
			ComponentDataAttribute systemAttributes = ReflectionUtils.GetAttributesOfType<ComponentDataAttribute>(componentDataType);
			if (systemAttributes == null)
			{
				return typeof(BasicCompContainer<>).MakeGenericType(componentDataType);
			}

			Type customContainerType = systemAttributes.ContainerType;
			if (customContainerType == typeof(BasicCompContainer<>))
			{
				return typeof(BasicCompContainer<>).MakeGenericType(componentDataType);
			}
			
			var containerInterfaceType = typeof(ComponentContainer<>).MakeGenericType(componentDataType);
			if (!customContainerType.IsClass || 
				!customContainerType.IsTypeOf<IGenericComponentContainer>() ||
				!customContainerType.IsTypeOf(containerInterfaceType))
			{
				Debug.LogError($"Invalid custom component container given for component type {componentDataType} "); 
			}
			
			return customContainerType;
		}
		
		private IEnumerable<(object, Type)> CreateAllComponentContainers(uint maxNumber)
		{
			IEnumerable<Type> allComponentDataTypes = TypeCache.Get().GetAllComponentDataTypes();
			
			foreach (Type componentDataType in allComponentDataTypes)
			{
				Type customContainerType = GetCustomContainerType(componentDataType);
				object newComponentContainer = Activator.CreateInstance(customContainerType, maxNumber);
				yield return (newComponentContainer, componentDataType);
				
				//
				//
				//
				// ComponentDataAttribute system =
				// 	ReflectionUtils.GetAttributesOfType<ComponentDataAttribute>(componentDataType);
				//
				// object newComponentContainer;
				//
				// if(system == null)
				// {
				// 	var containerType = typeof(BasicCompContainer<>).MakeGenericType(componentDataType);
				// 	newComponentContainer = Activator.CreateInstance(containerType, maxNumber);
				// 	yield return (newComponentContainer, componentDataType);
				// 	continue;
				// }
				//
				// Type customContainerType = system.ContainerType;
				// if (customContainerType == typeof(BasicCompContainer<>))
				// {
				// 	var containerType = typeof(BasicCompContainer<>).MakeGenericType(componentDataType);
				// 	newComponentContainer = Activator.CreateInstance(containerType, maxNumber);
				// 	yield return (newComponentContainer, componentDataType);
				// 	
				// 	continue;
				// }
				//
				// var containerInterfaceType = typeof(ComponentContainer<>).MakeGenericType(componentDataType);
				// if (!customContainerType.IsClass || 
				// 	!customContainerType.IsTypeOf<IGenericComponentContainer>() ||
				// 	!customContainerType.IsTypeOf(containerInterfaceType))
				// {
				// 	Debug.LogError($"Invalid custom component container given for component type {componentDataType} "); 
				// 	
				// 	var defaultContainerType = typeof(BasicCompContainer<>).MakeGenericType(componentDataType);
				// 	object defaultNewComponentContainer = Activator.CreateInstance(defaultContainerType, maxNumber);
				// 	yield return (defaultNewComponentContainer, componentDataType);
				//
				// 	continue;
				// }
				//
				// if (customContainerType.IsGenericType)
				// {
				// 	var customContainerTypeFullDefinition = customContainerType.MakeGenericType(componentDataType);
				// 	newComponentContainer = Activator.CreateInstance(customContainerTypeFullDefinition, maxNumber);
				// } else
				// {
				// 	newComponentContainer = Activator.CreateInstance(customContainerType, maxNumber);
				// }
				//
				// yield return (newComponentContainer, componentDataType);
			}
		}
		
		#endregion

	}
	
	
}