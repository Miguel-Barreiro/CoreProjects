#nullable enable

namespace Core.VSEngine
{
    public interface IValueNode
    {
        public object? GetValue(string portName);
    }
}