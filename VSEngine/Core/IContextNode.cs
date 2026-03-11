using Core;

namespace Core.VSEngine
{
    public interface IContextNode<T> where T : ExecutionContext
    {
        public T? Context { get; set; }
    }
}
