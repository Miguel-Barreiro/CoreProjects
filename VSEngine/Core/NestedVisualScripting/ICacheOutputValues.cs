using System.Collections.Generic;

#nullable enable

namespace VSEngine.Core.NestedVisualScripting
{
    public interface ICacheOutputValues
    {
        public void CacheOutputValues(Dictionary<string, object?> outputValues);
    }
}