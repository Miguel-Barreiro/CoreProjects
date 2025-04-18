using System;
using System.Collections.Generic;
using System.Linq;
using Core.Zenject.Source.Injection;
using Core.Zenject.Source.Internal;

namespace Core.Zenject.Source.Util
{
    public static class ValidationUtil
    {
        // This method can be used during validation for cases where we need to pass arguments
        public static List<TypeValuePair> CreateDefaultArgs(params Type[] argTypes)
        {
            return argTypes.Select(x => new TypeValuePair(x, x.GetDefaultValue())).ToList();
        }
    }
}

