using System;
using System.Collections.Generic;
using Core.Zenject.Source.Injection;
using Core.Zenject.Source.Internal;
using Core.Zenject.Source.Main;
using Zenject;

namespace Core.Zenject.Source.Providers.SubContainerCreators
{
    [NoReflectionBaking]
    public class SubContainerCreatorByInstanceGetter : ISubContainerCreator
    {
        readonly Func<InjectContext, DiContainer> _subcontainerGetter;

        public SubContainerCreatorByInstanceGetter(
            Func<InjectContext, DiContainer> subcontainerGetter)
        {
            _subcontainerGetter = subcontainerGetter;
        }

        public DiContainer CreateSubContainer(List<TypeValuePair> args, InjectContext context, out Action injectAction)
        {
            Assert.That(args.IsEmpty());

            injectAction = null;

            // It is assumed here that the subcontainer has already had ResolveRoots called elsewhere
            // Since most likely you are adding a subcontainer that is already in a context or
            // something rather than directly using DiContainer.CreateSubContainer
            return _subcontainerGetter(context);
        }
    }
}

