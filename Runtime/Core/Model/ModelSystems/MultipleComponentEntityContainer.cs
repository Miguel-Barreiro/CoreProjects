using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Core.Model.ModelSystems
{
	
	public struct EntityComponentsHeader
	{
		public const int MAX_COMPONENTS = 10;
		
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=MAX_COMPONENTS)] public uint[] Indexes;
		public uint Count;

		public static EntityComponentsHeader NEW(uint firstComponentIndex)
		{
			EntityComponentsHeader newEntityComponentsHeader = new EntityComponentsHeader()
			{
				Count = 1,
				Indexes = new uint[MAX_COMPONENTS],
			};
			newEntityComponentsHeader.Indexes[0] = firstComponentIndex;
				
			return newEntityComponentsHeader;
		}
		public void AddIndex(uint index)
		{
			if (Count >= MAX_COMPONENTS)
			{
				Debug.LogError($"EntityHeader: Cannot add more than {MAX_COMPONENTS} components");
				return;
			}
			Indexes[Count] = index;
			Count++;
		}

		public void SetNewIndex(uint lastComponentIndex, uint newIndex)
		{
			for (int i = 0; i < Count; i++)
			{
				if (Indexes[i] == lastComponentIndex)
				{
					Indexes[i] = newIndex;
					return;
				}
			}
		}
	}

	public abstract class MultipleComponentEntityContainer<T> : ComponentContainer<T>
		where T : struct, IComponentData
	{
		

		public T[] Components = null;
		public uint TopEmptyIndex => _topEmptyIndex;
		protected uint _topEmptyIndex = 0;
		
		public EntityComponentsHeader[] EntityComponentsHeaders = null;
		public uint TopEntityEmptyIndex => _topEntityEmptyIndex;
		protected uint _topEntityEmptyIndex = 0;
		
		protected readonly Dictionary<EntId, uint> ComponentIndexByOwner = new Dictionary<EntId, uint>();

		/// This is a dummy component to return when the requested component is not found
		/// do not override it
		public T Invalid = new T()
		{
			ID = EntId.Invalid,
		};
		public EntityComponentsHeader InvalidHeader = new EntityComponentsHeader()
		{
			Count = 0,
			Indexes = new uint[EntityComponentsHeader.MAX_COMPONENTS],
		};


		protected MultipleComponentEntityContainer(uint maxNumber)
		{
			_topEmptyIndex = 0;
			_topEntityEmptyIndex = 0;
			uint totalComponents = maxNumber * EntityComponentsHeader.MAX_COMPONENTS;
			Components = new T[totalComponents];
			for (int i = 0; i < totalComponents; i++)
				Components[i].Reset();
			
			EntityComponentsHeaders = new EntityComponentsHeader[maxNumber];
		}
		
		public virtual void SetupComponent(EntId owner)
		{
			if (ComponentIndexByOwner.ContainsKey(owner))
			{
				Debug.LogError($"Component({typeof(T)}) already exists for owner {owner}");
				return;
			}
			if(_topEntityEmptyIndex >= EntityComponentsHeaders.Length)
			{
				Debug.LogError($"No available space for new entity header({typeof(T)}), FILLED[{_topEntityEmptyIndex}/{EntityComponentsHeaders.Length}] ");
				return;
			}
			// int index = Array.FindIndex(_components, component => component.ID == EntId.Invalid);
			if (_topEmptyIndex >= Components.Length)
			{
				Debug.LogError($"No available space for new component({typeof(T)}), FILLED[{_topEmptyIndex}/{Components.Length}] ");
				return;
			}
			
			Components[_topEmptyIndex].Init();
			Components[_topEmptyIndex].ID = owner;
			ComponentIndexByOwner[owner] = _topEntityEmptyIndex;
			EntityComponentsHeaders[_topEntityEmptyIndex] = EntityComponentsHeader.NEW(_topEmptyIndex);
			
			_topEntityEmptyIndex++;
			_topEmptyIndex++;
		}
		
		public virtual uint AddComponent(EntId owner)
		{
			if (!ComponentIndexByOwner.TryGetValue(owner, out uint entityHeaderIndex))
			{
				Debug.LogError($"Component({typeof(T)}) does not exist for owner {owner}");
				return uint.MaxValue;
			}
			ref EntityComponentsHeader entityHeader = ref EntityComponentsHeaders[entityHeaderIndex];
			if(entityHeader.Count >= EntityComponentsHeader.MAX_COMPONENTS)
			{
				Debug.LogError($"Entity({owner}) already has maximum number of components({EntityComponentsHeader.MAX_COMPONENTS})");
				return uint.MaxValue;
			}
			
			if (_topEmptyIndex >= Components.Length)
			{
				Debug.LogError($"No available space for new component({typeof(T)}), FILLED[{_topEmptyIndex}/{Components.Length}] ");
				return uint.MaxValue;
			}
			
			Components[_topEmptyIndex].ID = owner;
			entityHeader.AddIndex(_topEmptyIndex);
			Components[_topEmptyIndex].Init();

			uint newIndex = _topEmptyIndex;
			_topEmptyIndex++;
			
			return newIndex;
		}

		public virtual void RemoveComponent(EntId owner)
		{
			if (!ComponentIndexByOwner.TryGetValue(owner, out uint entityHeaderIndex))
				return;

			ref EntityComponentsHeader entityHeader = ref EntityComponentsHeaders[entityHeaderIndex];

			for (int i = 0; i < entityHeader.Count; i++)
				RemoveComponentInternal(entityHeader.Indexes[i]);

			uint topEntityEmptyIndex = _topEntityEmptyIndex - 1;
			uint lastEntityIndex = EntityComponentsHeaders[topEntityEmptyIndex].Indexes[0];
			EntId topEntityID = Components[lastEntityIndex].ID;
			
			EntityComponentsHeaders[entityHeaderIndex] = EntityComponentsHeaders[topEntityEmptyIndex];
			ComponentIndexByOwner[topEntityID] = entityHeaderIndex;
			
			
			_topEntityEmptyIndex--;
			ComponentIndexByOwner.Remove(owner);
		}

		protected virtual void RemoveComponentInternal(uint index)
		{
			if (_topEmptyIndex <= 1)
			{
				if(index > 1)
					return;
				Components[index].ID = EntId.Invalid;
				_topEmptyIndex--;
				return;
			}
			
			
			uint topIndex = _topEmptyIndex-1;
			EntId topEntID = Components[topIndex].ID;
			uint topEntityIndex = ComponentIndexByOwner[topEntID];
			ref EntityComponentsHeader entityComponentsHeader = ref EntityComponentsHeaders[topEntityIndex];

			entityComponentsHeader.SetNewIndex(topIndex, index);
			Components[index] = Components[topIndex];

			Components[topIndex].ID = EntId.Invalid;
			_topEmptyIndex--;
		}


		public void SetupInvalid()
		{
			Invalid.Reset();
		}

		public virtual ref T GetComponent(EntId owner)
		{
			if (!ComponentIndexByOwner.TryGetValue(owner, out uint entityHeaderIndex))
			{
				// Debug.LogError($"Component({typeof(T)}) not found for owner {owner}");
				return ref Invalid;
			}
			
			return ref Components[EntityComponentsHeaders[entityHeaderIndex].Indexes[0]];
		}

		public virtual ref EntityComponentsHeader GetEntityComponentsHeader(EntId owner)
		{
			if (!ComponentIndexByOwner.TryGetValue(owner, out uint entityHeaderIndex))
			{
				Debug.LogError($"Component({typeof(T)}) not found for owner {owner}");
				return ref InvalidHeader;
			}
			
			return ref EntityComponentsHeaders[entityHeaderIndex];
		}
		public virtual ref T GetComponent(uint index)
		{
			return ref Components[index];
		}


		public virtual void RebuildWithMax(uint maxNumber)
		{
			_topEmptyIndex = 0;
			_topEntityEmptyIndex = 0;
			
			ComponentIndexByOwner.Clear();
			
			EntityComponentsHeaders = new EntityComponentsHeader[maxNumber];

			uint maxComponents = maxNumber * EntityComponentsHeader.MAX_COMPONENTS;
			Components = new T[maxComponents];
			for (int i = 0; i < maxComponents; i++)
				Components[i].Reset();
		}


		public uint Count => _topEmptyIndex;
		public uint MaxCount => (uint) Components.Length;
	}
}