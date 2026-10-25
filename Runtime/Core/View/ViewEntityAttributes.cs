using System;
using System.Collections.Generic;
using Core.Model;
using Core.Zenject.Source.Factories.Pooling.Static;
using UnityEngine;
	
#nullable enable

namespace Core.View
{
	public sealed class EntityViewAtributes : IDisposable
	{
		
		internal static readonly StaticMemoryPool<EntityViewAtributes> Pool =
			new StaticMemoryPool<EntityViewAtributes>(OnSpawned, OnDespawned, OnAlloc);
		
		public static EntityViewAtributes NEW(EntId id)
		{
			var instance = Pool.Spawn();
			instance.GameObject = null;
			instance.ID = id;
			return instance;
		}
		
		
		private static void OnAlloc(EntityViewAtributes obj) { }
		
		private static void OnDespawned(EntityViewAtributes obj) { }

		private static void OnSpawned(EntityViewAtributes obj) { }

		public void Dispose()
		{
			Pool.Despawn(this as EntityViewAtributes);
		}


		public EntId ID { get; private set; }
		public GameObject? GameObject { get; internal set; }

		private readonly Dictionary<Type, object?> ComponentCache = new ();
		public EntityViewAtributes() { }

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