using System;
using System.Collections.Generic;
using Core.Zenject.Source.Injection;
using Core.Zenject.Source.Internal;
using Core.Zenject.Source.Main;
using Zenject;

namespace Core.Zenject.Source.Providers.SubContainerCreators
{
    [NoReflectionBaking]
    public class SubContainerCreatorByInstance : ISubContainerCreator
    {
        readonly DiContainer _subcontainer;

        public SubContainerCreatorByInstance(DiContainer subcontainer)
        {
            _subcontainer = subcontainer;
        }

        public DiContainer CreateSubContainer(List<TypeValuePair> args, InjectContext context, out Action injectAction)
        {
            Assert.That(args.IsEmpty());

            injectAction = null;

            // It is assumed here that the subcontainer has already had ResolveRoots called elsewhere
            // Since most likely you are adding a subcontainer that is already in a context or
            // something rather than directly using DiContainer.CreateSubContainer
            return _subcontainer;
        }
    }
}

