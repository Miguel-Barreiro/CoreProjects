using System;

#nullable enable

namespace Core.Utils
{
    public class Result<T>
    {
        public T? Value { get; }
        public bool IsFail { get; }
        public Exception? Error { get; }
        
        public Result(Exception error)
        {
            Value = default(T);
            IsFail = true;
            Error = error;
        }
        public Result(T value)
        {
            Value = value;
            IsFail = false;
            Error = null;
        }
    }
}