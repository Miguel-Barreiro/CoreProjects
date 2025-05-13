using System;
using System.Collections.Generic;
using Core.Model.ModelSystems;
using UnityEngine;


namespace Core.Model
{
	public interface IGenericComponentContainer
	{
		public void SetupComponent(EntId owner);
		public void RemoveComponent(EntId owner);
	}

	public interface ComponentContainer<T>
		where T : struct, IComponentData
	{

		public ref T GetComponent(EntId owner);
		
		// public void RebuildWithMax(int maxNumber);
		
		public bool MoveNext();
		public ref T GetCurrent();
		public void ResetIterator();
	}

	
	
	
	public class ComponentContainerImplementation<T> : ComponentContainer<T>, IGenericComponentContainer
		where T : struct, IComponentData
	{
		private T[] _components = null;
		private int _iteratorIndex = 0;

		private readonly Dictionary<EntId, uint> ComponentIndexByOwner = new Dictionary<EntId, uint>();
		
		/// This is a dummy component to return when the requested component is not found
		/// do not override it
		public T Invalid = new T()
		{
			ID = EntId.Invalid
		};
		
		
		public ComponentContainerImplementation(uint maxNumber)
		{
			_components = new T[maxNumber];
			for (uint i = 0; i < _components.Length; i++)
			{
				_components[i].ID = EntId.Invalid;
			}
		}
		
		public void SetupComponent(EntId owner)
		{
			if (ComponentIndexByOwner.ContainsKey(owner))
			{
				Debug.LogError($"Component({{typeof(T)}}) already exists for owner {owner}");
				return;
			}

			int index = Array.FindIndex(_components, component => component.ID == EntId.Invalid);
			if (index == -1)
			{
				Debug.LogError($"No available space for new component({typeof(T)})");
				return;
			}
			
			_components[index].ID = owner;
			ComponentIndexByOwner[owner] = (uint) index;

			_components[index].Init();
		}

		public void RemoveComponent(EntId owner)
		{
			if (!ComponentIndexByOwner.TryGetValue(owner, out uint index))
				return;

			_components[index].ID = EntId.Invalid;
			ComponentIndexByOwner.Remove(owner);
		}
		
		public ref T GetComponent(EntId owner)
		{
			if (!ComponentIndexByOwner.TryGetValue(owner, out uint index))
			{
				Debug.LogError($"Component({{typeof(T)}}) not found for owner {owner}");
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
			_iteratorIndex++;
			if (_iteratorIndex >= _components.Length)
				return false;

			ref T component = ref _components[_iteratorIndex];
			while ( component.ID == EntId.Invalid)
			{
				_iteratorIndex++;
				if (_iteratorIndex >= _components.Length)
					return false;

				component = ref _components[_iteratorIndex];
			}

			return true;
		}

		
		public ref T GetCurrent()
		{
			if (_iteratorIndex < 0 || _iteratorIndex >= _components.Length)
				_iteratorIndex = 0;

			return ref _components[_iteratorIndex];
		}

		
		public void ResetIterator()
			=> _iteratorIndex = -1;


	}

}