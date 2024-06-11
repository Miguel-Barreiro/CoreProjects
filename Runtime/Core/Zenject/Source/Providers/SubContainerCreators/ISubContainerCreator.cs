using System;
using System.Collections.Generic;
using Core.Zenject.Source.Injection;
using Core.Zenject.Source.Main;

namespace Core.Zenject.Source.Providers.SubContainerCreators
{
    public interface ISubContainerCreator
    {
        DiContainer CreateSubContainer(List<TypeValuePair> args, InjectContext context, out Action injectAction);
    }
}
