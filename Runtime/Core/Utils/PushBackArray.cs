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
		private T[] items;
		public uint count;

		public T[] Items=> items;
		
		public uint Count => count;
		public uint MaxCount => (uint) items.Length;
		
		public PushBackArray(uint size)
		{
			items = new T[size];
			this.count = 0;
		}
		
		public bool SetupNew(out uint newIndex)
		{
			if(count >= items.Length)
			{
				Debug.LogError($"no space for new item, count: {count}, max: {items.Length}"); 
				newIndex = uint.MaxValue;
				return false;
			}

			newIndex = count;
			items[newIndex] = default(T);
			count++;
			
			return true;
		}

		public bool Add(T item, out uint newIndex)
		{
			if(count >= items.Length)
			{
				Debug.LogError($"no space for new item, count: {count}, max: {items.Length}"); 
				newIndex = uint.MaxValue;
				return false;
			}

			newIndex = count;
			items[newIndex] = item;
			count++;
			
			return true;
		}

		public ref T Remove(uint index, out uint changedIndex)
		{
			if(index >= count)
			{
				Debug.LogError($"index out of range, index: {index}, count: {count}");
				changedIndex = uint.MaxValue;
				return ref items[0];
			}

			uint top = count - 1;
			items[index] = items[top];
			count--;

			changedIndex = index;
			return ref items[index]; 
		}
		
		public ref T? GetAt(int index)
		{
			if(index >= count)
			{
				Debug.LogError($"index out of range, index: {index}, count: {count}");
				return ref items[0];
			}
			
			return ref items[index];
		}
		

		public void RebuildWithSize(uint size)
		{
			count = 0;
			
			if (items.Length == size)
				return;
			
			items = new T[size];
		}
	}
}