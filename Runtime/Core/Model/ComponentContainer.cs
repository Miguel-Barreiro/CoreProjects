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

	// public interface ComponentContainer<T>
	// 	where T : struct, IComponentData
	// {
	//
	// 	public 
	// 	
	// 	public ref T GetComponent(EntId owner);
	// 	
	// 	// public void RebuildWithMax(int maxNumber);
	// 	
	// 	// public bool MoveNext();
	// 	// public ref T GetCurrent();
	// 	// public void ResetIterator();
	//
	// 	public uint Count { get; }
	// 	
	// 	public uint MaxCount  { get; }
	// }

	
	
	
	public class ComponentContainer<T> : IGenericComponentContainer
		where T : struct, IComponentData
	{
		public T[] Components = null;
		// private int _iteratorIndex = 0;
		
		public uint TopEmptyIndex = 0;

		private readonly Dictionary<EntId, uint> ComponentIndexByOwner = new Dictionary<EntId, uint>();
		
		/// This is a dummy component to return when the requested component is not found
		/// do not override it
		public T Invalid = new T()
		{
			ID = EntId.Invalid
		};
		
		
		public ComponentContainer(uint maxNumber)
		{
			TopEmptyIndex = 0;
			Components = new T[maxNumber];
			// for (uint i = 0; i < _components.Length; i++)
			// {
			// 	_components[i].ID = EntId.Invalid;
			// }
		}
		
		public void SetupComponent(EntId owner)
		{
			if (ComponentIndexByOwner.ContainsKey(owner))
			{
				Debug.LogError($"Component({typeof(T)}) already exists for owner {owner}");
				return;
			}

			
			// int index = Array.FindIndex(_components, component => component.ID == EntId.Invalid);
			if (TopEmptyIndex >= Components.Length)
			{
				Debug.LogError($"No available space for new component({typeof(T)}), FILLED[{TopEmptyIndex}/{Components.Length}] ");
				return;
			}
			
			Components[TopEmptyIndex].ID = owner;
			ComponentIndexByOwner[owner] = TopEmptyIndex;
			Components[TopEmptyIndex].Init();
			
			TopEmptyIndex++;
		}

		public void RemoveComponent(EntId owner)
		{
			if (!ComponentIndexByOwner.TryGetValue(owner, out uint index))
				return;

			if (TopEmptyIndex <= 1)
			{
				Components[index].ID = EntId.Invalid;
				TopEmptyIndex--;
				ComponentIndexByOwner.Remove(owner);
				return;
			}

			
			uint lastComponentIndex = TopEmptyIndex-1;
			EntId topEntityID = Components[lastComponentIndex].ID;

			ComponentIndexByOwner[topEntityID] = index;
			Components[index] = Components[lastComponentIndex];

			Components[lastComponentIndex].ID = EntId.Invalid;
			TopEmptyIndex--;
			ComponentIndexByOwner.Remove(owner);
		}
		
		public ref T GetComponent(EntId owner)
		{
			if (!ComponentIndexByOwner.TryGetValue(owner, out uint index))
			{
				Debug.LogError($"Component({typeof(T)}) not found for owner {owner}");
				return ref Invalid;
			}
			
			return ref Components[index];
		}
		

		public void RebuildWithMax(int maxNumber)
		{
			TopEmptyIndex = 0;
			ComponentIndexByOwner.Clear();
			
			Components = new T[maxNumber];
			// for (int i = 0; i < _components.Length; i++)
			// {
			// 	_components[i].ID = EntId.Invalid;
			// }
		}


		//
		// public bool MoveNext()
		// { 
		// 	_iteratorIndex++;
		// 	if (_iteratorIndex >= _topEmptyIndex)
		// 		return false;
		//
		// 	// _iteratorIndex++;
		// 	// ref T component = ref _components[_iteratorIndex];
		// 	// while ( component.ID == EntId.Invalid)
		// 	// {
		// 	// 	if (_iteratorIndex >= _components.Length)
		// 	// 		return false;
		// 	//
		// 	// 	component = ref _components[_iteratorIndex];
		// 	// }
		// 	return true;
		// }
		//
		//
		// public ref T GetCurrent()
		// {
		// 	if (_iteratorIndex < 0 || _iteratorIndex >= _topEmptyIndex)
		// 		_iteratorIndex = 0;
		//
		// 	return ref Components[_iteratorIndex];
		// }
		//
		// public void ResetIterator()
		// 	=> _iteratorIndex = -1;

		public uint Count => TopEmptyIndex;
		public uint MaxCount => (uint) Components.Length;

	}

}