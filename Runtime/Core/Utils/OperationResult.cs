using System;

namespace Core.Utils
{
    public class OperationResult<T>
    {
        public bool IsSuccess { get; private set; }
        public bool IsFailure => !IsSuccess;
        public Exception Exception { get; private set; }
        public T Result { get; private set; }

        private OperationResult(bool isSuccess, Exception e, T result)
        {
            IsSuccess = isSuccess;
            Exception = e;
            Result = result;
        }

        public static OperationResult<T> Success(T result)
        {
            return new OperationResult<T>(true, null, result);
        }

        public static OperationResult<T> Failure(Exception e)
        {
            return new OperationResult<T>(false, e, default(T));
        }
        
        public static OperationResult<T> Failure(string message)
        {
            return new OperationResult<T>(false, new OperationException(message), default(T));
        }
        
    }
    
    
    public class OperationResult
    {
        public bool IsSuccess { get; private set; }
        public bool IsFailure => !IsSuccess;
        public Exception Exception { get; private set; }

        private OperationResult(bool isSuccess, Exception e)
        {
            IsSuccess = isSuccess;
            Exception = e;
        }

        public static OperationResult Success()
        {
            return new OperationResult(true, null);
        }

        public static OperationResult Failure(Exception e)
        {
            return new OperationResult(false, e);
        }
        
        public static OperationResult Failure(string message)
        {
            return new OperationResult(false, new OperationException(message));
        }
        
    }
    
    public class OperationException : Exception
    {
        public OperationException(string message) : base(message) { }
    }
}
