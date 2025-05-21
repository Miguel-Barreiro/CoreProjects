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
		
		public void RebuildWithMax(uint maxNumber);
	}
	
	public interface ComponentContainer<T> : IGenericComponentContainer
		where T : struct, IComponentData
	{
		
		public ref T GetComponent(EntId owner);

		public uint Count { get; }
		public uint MaxCount  { get; }
	}
	
	
	public class BasicCompContainer<T> : IGenericComponentContainer, ComponentContainer<T>
		where T : struct, IComponentData
	{
		public T[] Components = null;
		public uint TopEmptyIndex => _topEmptyIndex;
		

		private readonly Dictionary<EntId, uint> ComponentIndexByOwner = new Dictionary<EntId, uint>();
		private uint _topEmptyIndex = 0;
		
		/// This is a dummy component to return when the requested component is not found
		/// do not override it
		public T Invalid = new T()
		{
			ID = EntId.Invalid
		};
		
		
		public BasicCompContainer(uint maxNumber)
		{
			_topEmptyIndex = 0;
			Components = new T[maxNumber];
		}
		
		public void SetupComponent(EntId owner)
		{
			if (ComponentIndexByOwner.ContainsKey(owner))
			{
				Debug.LogError($"Component({typeof(T)}) already exists for owner {owner}");
				return;
			}
			
			// int index = Array.FindIndex(_components, component => component.ID == EntId.Invalid);
			if (_topEmptyIndex >= Components.Length)
			{
				Debug.LogError($"No available space for new component({typeof(T)}), FILLED[{_topEmptyIndex}/{Components.Length}] ");
				return;
			}
			
			Components[_topEmptyIndex].ID = owner;
			ComponentIndexByOwner[owner] = _topEmptyIndex;
			Components[_topEmptyIndex].Init();
			
			_topEmptyIndex++;
		}

		public void RemoveComponent(EntId owner)
		{
			if (!ComponentIndexByOwner.TryGetValue(owner, out uint index))
				return;

			if (_topEmptyIndex <= 1)
			{
				Components[index].ID = EntId.Invalid;
				_topEmptyIndex--;
				ComponentIndexByOwner.Remove(owner);
				return;
			}

			
			uint lastComponentIndex = _topEmptyIndex-1;
			EntId topEntityID = Components[lastComponentIndex].ID;

			ComponentIndexByOwner[topEntityID] = index;
			Components[index] = Components[lastComponentIndex];

			Components[lastComponentIndex].ID = EntId.Invalid;
			_topEmptyIndex--;
			ComponentIndexByOwner.Remove(owner);
		}


		public ref T GetComponent(EntId owner)
		{
			if (!ComponentIndexByOwner.TryGetValue(owner, out uint index))
			{
				// Debug.LogError($"Component({typeof(T)}) not found for owner {owner}");
				return ref Invalid;
			}
			
			return ref Components[index];
		}
		

		public void RebuildWithMax(uint maxNumber)
		{
			_topEmptyIndex = 0;
			ComponentIndexByOwner.Clear();
			
			Components = new T[maxNumber];
		}


		public uint Count => _topEmptyIndex;
		public uint MaxCount => (uint) Components.Length;

	}

}