using System.Collections.Generic;

#nullable enable

namespace Core.Utils
{
	public class ListStack<TType> : List<TType>, IListStack<TType>
	{
		public void Push(TType item)
		{
			this.Add(item);
		}

		public TType? Pop()
		{
			if (Count == 0)
			{
				return default;
			}

			TType result = this[0];
			this.RemoveAt(0);
			return result;
		}

		public TType? Top() => Count > 0? this[0] : default;
	}
	
	public interface IListStack<TType> : IList<TType>
	{
		public void Push(TType item);
		public TType? Pop();

		public TType? Top();
	}
	
}