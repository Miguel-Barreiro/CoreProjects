using UnityEngine;

namespace Core.Utils
{
	public interface IPushBackArray<T>
	{
		public bool SetupNew(out uint newIndex);
		public bool Add(T item, out uint newIndex);
		public ref T Remove(uint index, out uint changedIndex);
		public ref T GetAt(int index);
		
		public uint Count { get; }
		public uint MaxCount { get; }
		
		public void RebuildWithSize(uint size);
	}


	public struct PushBackArray<T> : IPushBackArray<T>
	{
		public T[] Items;
		public uint count;

		public PushBackArray(uint size)
		{
			Items = new T[size];
			this.count = 0;
		}
		
		public ref T this[uint i]
		{
			get => ref Items[i];
		}

		public bool SetupNew(out uint newIndex)
		{
			if(count >= Items.Length)
			{
				Debug.LogError($"no space for new item, count: {count}, max: {Items.Length}"); 
				newIndex = uint.MaxValue;
				return false;
			}

			newIndex = count;
			Items[newIndex] = default(T);
			count++;
			
			return true;
		}

		public bool Add(T item, out uint newIndex)
		{
			if(count >= Items.Length)
			{
				Debug.LogError($"no space for new item, count: {count}, max: {Items.Length}"); 
				newIndex = uint.MaxValue;
				return false;
			}

			newIndex = count;
			Items[newIndex] = item;
			count++;
			
			return true;
		}

		public ref T Remove(uint index, out uint changedIndex)
		{
			if(index >= count)
			{
				Debug.LogError($"index out of range, index: {index}, count: {count}");
				changedIndex = uint.MaxValue;
				return ref Items[0];
			}

			uint top = count - 1;
			Items[index] = Items[top];
			count--;

			changedIndex = index;
			return ref Items[index]; 
		}
		
		public ref T? GetAt(int index)
		{
			if(index >= count)
			{
				Debug.LogError($"index out of range, index: {index}, count: {count}");
				return ref Items[0];
			}
			
			return ref Items[index];
		}
		
		public uint Count => count;
		public uint MaxCount => (uint) Items.Length;

		public void RebuildWithSize(uint size)
		{
			count = 0;
			
			if (Items.Length == size)
				return;
			
			Items = new T[size];
		}
	}
}

//
// public void SetupComponent(EntId owner)
// 		{
// 			if (ComponentIndexByOwner.ContainsKey(owner))
// 			{
// 				Debug.LogError($"Component({typeof(T)}) already exists for owner {owner}");
// 				return;
// 			}
// 			
// 			// int index = Array.FindIndex(_components, component => component.ID == EntId.Invalid);
// 			if (_topEmptyIndex >= Components.Length)
// 			{
// 				Debug.LogError($"No available space for new component({typeof(T)}), FILLED[{_topEmptyIndex}/{Components.Length}] ");
// 				return;
// 			}
// 			
// 			Components[_topEmptyIndex].ID = owner;
// 			ComponentIndexByOwner[owner] = _topEmptyIndex;
// 			Components[_topEmptyIndex].Init();
// 			
// 			_topEmptyIndex++;
// 		}
//
// 		public void RemoveComponent(EntId owner)
// 		{
// 			if (!ComponentIndexByOwner.TryGetValue(owner, out uint index))
// 				return;
//
// 			if (_topEmptyIndex <= 1)
// 			{
// 				Components[index].ID = EntId.Invalid;
// 				_topEmptyIndex--;
// 				ComponentIndexByOwner.Remove(owner);
// 				return;
// 			}
//
// 			
// 			uint lastComponentIndex = _topEmptyIndex-1;
// 			EntId topEntityID = Components[lastComponentIndex].ID;
//
// 			ComponentIndexByOwner[topEntityID] = index;
// 			Components[index] = Components[lastComponentIndex];
//
// 			Components[lastComponentIndex].ID = EntId.Invalid;
// 			_topEmptyIndex--;
// 			ComponentIndexByOwner.Remove(owner);
// 		}
// 		
// 		public ref T GetComponent(EntId owner)
// 		{
// 			if (!ComponentIndexByOwner.TryGetValue(owner, out uint index))
// 			{
// 				Debug.LogError($"Component({typeof(T)}) not found for owner {owner}");
// 				return ref Invalid;
// 			}
// 			
// 			return ref Components[index];
// 		}
// 		
//
// 		public void RebuildWithMax(int maxNumber)
// 		{
// 			_topEmptyIndex = 0;
// 			ComponentIndexByOwner.Clear();
// 			
// 			Components = new T[maxNumber];
// 			// for (int i = 0; i < _components.Length; i++)
// 			// {
// 			// 	_components[i].ID = EntId.Invalid;
// 			// }
// 		}
//
//
// 		public uint Count => _topEmptyIndex;
// 		public uint MaxCount => (uint) Components.Length;
// 		