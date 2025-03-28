using System;
using System.Collections.Generic;
using Core.Model;
using UnityEngine;
	
#nullable enable

namespace Core.View
{
	public class EntityViewAtributes
	{
		public EntityViewAtributes(EntId id)
		{
			GameObject = null;
			ID = id;
		}

		public EntId ID { get; }
		public GameObject? GameObject { get; internal set; }

		private readonly Dictionary<Type, object?> ComponentCache = new ();
		
		// public T? Get<T>() 
		// 	where T : Component
		// {
		// 	if (GameObject == null) return default(T);
		// 	
		// 	Type componentType = typeof(T);
		// 	if (ComponentCache.TryGetValue(componentType, out object? value))
		// 		return value as T;
		//
		// 	T component = GameObject.GetComponent<T>();
		// 	if (component == null)
		// 	{
		// 		component = GameObject.GetComponentInChildren<T>();
		// 		if (component == null)
		// 		{
		// 			Debug.LogError($"did not find {componentType.Name} component in {GameObject.name}");
		// 			return null;
		// 		}
		// 	}
		// 	
		// 	ComponentCache.Add(componentType, component);
		// 	return component;
		// }

		
		public T? Get<T>()
		{
			if (GameObject == null) return default(T);
			
			Type componentType = typeof(T);
			if (ComponentCache.TryGetValue(componentType, out object? value))
				return (T?)value;
			
			T? component = GameObject.GetComponent<T>();
			if (component == null)
			{
				component = GameObject.GetComponentInChildren<T>();
				if (component == null)
				{
					// Debug.LogError($"did not find {componentType.Name} component in {GameObject.name}");
					
					component = default(T);
				}
			}
			ComponentCache.Add(componentType, component);
			
			return component;
		}
	}
}