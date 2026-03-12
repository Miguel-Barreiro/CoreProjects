#nullable enable

using Core.Utils;

namespace Core.VSEngine
{
    public interface IValueNode
    {
        public OperationResult<object> GetValue(string portName);
    }
}