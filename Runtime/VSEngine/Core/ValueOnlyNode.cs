#nullable enable


using Core.Utils;
using Core.VSEngine.Nodes;

namespace Core.VSEngine
{
    [NodeTint(VSNodeMenuNames.VALUES_NODES_TINT)]
    public abstract class ValueOnlyNode: VSNodeBase, IValueNode
    {
        public abstract OperationResult<object> GetValue(string portName);
        
        protected const string CACHE_RESULT_VARIABLE_NAME = "CACHE_RESULT_VARIABLE_NAME";
        
        protected void CacheResult(object filteredEntities)
        {
            SetVariable(CACHE_RESULT_VARIABLE_NAME, filteredEntities);
        }

        protected bool TryCache(out object? variable) 
        {
            if (HasVariable(CACHE_RESULT_VARIABLE_NAME))
            {
                variable = GetVariable(CACHE_RESULT_VARIABLE_NAME);
                return true;
            }
            variable = null;
            return false;
        }
    }
}