using System;
using Core.Zenject.Source.Factories.Pooling.Static;

namespace Core.Utils
{
    public class OperationResult<T> : IDisposable 
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

        public OperationResult() { }

        public static OperationResult<T> Success(T result)
        {
            OperationResult<T> operationResult = Pool.Spawn();
            operationResult.IsSuccess = true;
            operationResult.Exception = null;
            operationResult.Result = result;
            return operationResult;
        }

        public static OperationResult<T> Failure(Exception e)
        {
            OperationResult<T> operationResult = Pool.Spawn();
            operationResult.Exception = e;
            operationResult.Result = default(T);
            operationResult.IsSuccess = false;
            return operationResult;
        }
        
        public static OperationResult<T> Failure(string message)
        {
            OperationResult<T> operationResult = Pool.Spawn();
            operationResult.Exception = new OperationException(message);
            operationResult.Result = default(T);
            operationResult.IsSuccess = false;
            return operationResult;
        }
        
        protected static readonly Exception DefaultException = new OperationException("[DEFAULT]An error occurred during the operation.");
        protected static readonly StaticMemoryPool<OperationResult<T>> Pool =
            new StaticMemoryPool<OperationResult<T>>(OnSpawned, OnDespawned);

        private static void OnDespawned(OperationResult<T> obj)
        {
            obj.Exception = DefaultException;
        }

        private static void OnSpawned(OperationResult<T> obj) { }
        public void Dispose()
        {
            Pool.Despawn(this as OperationResult<T>);
        }
        
        
    }
    
    
    public class OperationResult : IDisposable
    {
        public bool IsSuccess { get; private set; }
        public bool IsFailure => !IsSuccess;
        
        private static readonly OperationResult SuccessInstance = new OperationResult(true, null);
        
        public Exception Exception { get; private set; }

        private OperationResult(bool isSuccess, Exception e)
        {
            IsSuccess = isSuccess;
            Exception = e;
        }

        public OperationResult()
        {
            
        }

        public static OperationResult Success() => SuccessInstance;

        public static OperationResult Failure(Exception e)
        {
            OperationResult operationResult = Pool.Spawn();
            operationResult.Exception = e;
            operationResult.IsSuccess = false;
            return operationResult;
        }
        
        public static OperationResult Failure(string message)
        {
            OperationResult operationResult = Pool.Spawn();
            operationResult.Exception = new OperationException(message);
            operationResult.IsSuccess = false;
            return operationResult;
        }

        
        protected static readonly Exception DefaultException = new OperationException("[DEFAULT]An error occurred during the operation.");
        protected static readonly StaticMemoryPool<OperationResult> Pool =
            new StaticMemoryPool<OperationResult>(OnSpawned, OnDespawned);

        private static void OnSpawned(OperationResult obj) { }
        private static void OnDespawned(OperationResult obj)
        {
            obj.Exception = DefaultException;
        }

        public void Dispose()
        {
            Pool.Despawn(this as OperationResult);
        }
    }
    
    public class OperationException : Exception
    {
        public OperationException(string message) : base(message) { }
    }
}
