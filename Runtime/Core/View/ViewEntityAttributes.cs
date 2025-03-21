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

		private readonly Dictionary<Type, MonoBehaviour> componentCache = new ();
		public T? Get<T>() 
			where T : MonoBehaviour
		{
			Type componentType = typeof(T);
			if (componentCache.TryGetValue(componentType, out MonoBehaviour? value))
			{
				return value as T;
			}

			T component = GameObject.GetComponent<T>();
			if (component == null)
			{
				component = GameObject.GetComponentInChildren<T>();
				if (component == null)
				{
					Debug.LogError($"did not find {componentType.Name} component in {GameObject.name}");
					return null;
				}
			}

			return component;
		}
	}
}