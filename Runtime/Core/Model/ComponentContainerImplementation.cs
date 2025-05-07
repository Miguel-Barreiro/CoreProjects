using System;
using System.Collections.Generic;
using Core.Model.ModelSystems;
using UnityEngine;


namespace Core.Model
{

	public interface ComponentContainer<T>
		where T : struct, IComponentData
	{

		public ref T GetComponent(EntId owner);
		
		public ref T AddComponent(EntId owner);
		public void RemoveComponent(EntId owner);

		public void RebuildWithMax(int maxNumber);


		public bool MoveNext();
		public ref T GetCurrent();
		public void ResetIterator();
	}

	
	
	
	public class ComponentContainerImplementation<T> : ComponentContainer<T> 
		where T : struct, IComponentData
	{
		private T[] _components = null;
		private int _index = 0;

		private readonly Dictionary<EntId, int> ComponentIndexByOwner = new Dictionary<EntId, int>();
		
		/// This is a dummy component to return when the requested component is not found
		/// do not override it
		public T Invalid = new T()
		{
			ID = EntId.Invalid
		};
		
		
		public ComponentContainerImplementation(int maxNumber)
		{
			_components = new T[maxNumber];
			for (int i = 0; i < _components.Length; i++)
			{
				_components[i].ID = EntId.Invalid;
			}
		}
		
		public ref T AddComponent(EntId owner)
		{
			if (ComponentIndexByOwner.ContainsKey(owner))
			{
				Debug.LogError($"Component already exists for owner {owner}");
				return ref Invalid;
			}

			int index = Array.FindIndex(_components, component => component.ID == EntId.Invalid);
			if (index == -1)
			{
				Debug.LogError("No available space for new component");
				return ref Invalid;
			}
			
			_components[index].ID = owner;
			ComponentIndexByOwner[owner] = index;

			return ref _components[index];
		}
		
		public void RemoveComponent(EntId owner)
		{
			if (ComponentIndexByOwner.ContainsKey(owner))
			{
				Debug.LogError($"Component already exists for owner {owner}");
				return;
			}

			int index = ComponentIndexByOwner[owner];
			_components[index].ID = EntId.Invalid;
			ComponentIndexByOwner[owner] = index;
		}
		
		public ref T GetComponent(EntId owner)
		{
			if (!ComponentIndexByOwner.TryGetValue(owner, out int index))
			{
				Debug.LogError($"Component not found for owner {owner}");
				return ref Invalid;
			}
			
			return ref _components[index];
		}

		public void RebuildWithMax(int maxNumber)
		{
			ComponentIndexByOwner.Clear();
			
			_components = new T[maxNumber];
			for (int i = 0; i < _components.Length; i++)
			{
				_components[i].ID = EntId.Invalid;
			}
		}



		public bool MoveNext()
		{ 
			_index++;
			if (_index >= _components.Length)
				return false;

			ref T component = ref _components[_index];
			while ( component.ID == EntId.Invalid)
			{
				_index++;
				if (_index >= _components.Length)
					return false;

				component = ref _components[_index];
			}

			return true;
		}

		
		public ref T GetCurrent()
		{
			if (_index < 0 || _index >= _components.Length)
				_index = 0;

			return ref _components[_index];
		}

		
		public void ResetIterator()
			=> _index = -1;

		
		
	}

}