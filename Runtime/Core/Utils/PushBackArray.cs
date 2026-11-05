using System.Runtime.InteropServices;
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


	public interface IArrayItems<T> 
	{
		public uint Length { get; }
		public T[] Items { get; }
	}

	public struct PushBackArray<T, TArray> : IPushBackArray<T> 
		where TArray : struct, IArrayItems<T>
	{
		// [MarshalAs(UnmanagedType.ByValArray, SizeConst=MAX_STATUS_EFFECTS)]
		// private T[] items;
		private TArray array;
		public uint count;

		public T[] Items=> array.Items;
		
		public uint Count => count;
		public uint MaxCount => (uint) array.Length;
		
		public PushBackArray(TArray itemsArray)
		{
			array = itemsArray;
			this.count = 0;
		}
		
		public bool SetupNew(out uint newIndex)
		{
			if(count >= array.Length)
			{
				Debug.LogError($"no space for new item, count: {count}, max: {array.Length}"); 
				newIndex = uint.MaxValue;
				return false;
			}

			newIndex = count;
			array.Items[newIndex] = default(T);
			count++;
			
			return true;
		}

		public bool Add(T item, out uint newIndex)
		{
			if(count >= array.Length)
			{
				Debug.LogError($"no space for new item, count: {count}, max: {array.Length}"); 
				newIndex = uint.MaxValue;
				return false;
			}

			newIndex = count;
			array.Items[newIndex] = item;
			count++;
			
			return true;
		}

		public ref T Remove(uint index, out uint changedIndex)
		{
			if(index >= count)
			{
				Debug.LogError($"index out of range, index: {index}, count: {count}");
				changedIndex = uint.MaxValue;
				return ref array.Items[0];
			}

			uint top = count - 1;
			array.Items[index] = array.Items[top];
			count--;

			changedIndex = index;
			return ref array.Items[index]; 
		}
		
		public ref T? GetAt(int index)
		{
			if(index >= count)
			{
				Debug.LogError($"index out of range, index: {index}, count: {count}");
				return ref array.Items[0];
			}
			
			return ref array.Items[index];
		}
		

		public void RebuildWithSize(uint size)
		{
			count = 0;
			
			// if (array.Items.Length == size)
			// 	return;
			// array.
			// array.Items = new T[size];
		}
	}
}