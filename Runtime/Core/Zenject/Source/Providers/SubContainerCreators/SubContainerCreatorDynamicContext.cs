#if !NOT_UNITY3D

using System;
using System.Collections.Generic;
using Core.Zenject.Source.Injection;
using Core.Zenject.Source.Install.Contexts;
using Core.Zenject.Source.Main;
using UnityEngine;
using Zenject;

namespace Core.Zenject.Source.Providers.SubContainerCreators
{
    [NoReflectionBaking]
    public abstract class SubContainerCreatorDynamicContext : ISubContainerCreator
    {
        readonly DiContainer _container;

        public SubContainerCreatorDynamicContext(DiContainer container)
        {
            _container = container;
        }

        protected DiContainer Container
        {
            get { return _container; }
        }

        public DiContainer CreateSubContainer(
            List<TypeValuePair> args, InjectContext parentContext, out Action injectAction)
        {
            bool shouldMakeActive;
            var gameObj = CreateGameObject(parentContext, out shouldMakeActive);

            var context = gameObj.AddComponent<GameObjectContext>();

            AddInstallers(args, context);

            context.Install(_container);

            injectAction = () => 
            {
                // Note: We don't need to call ResolveRoots here because GameObjectContext does this for us
                _container.Inject(context);

                if (shouldMakeActive && !_container.IsValidating)
                {
#if ZEN_INTERNAL_PROFILING
                    using (ProfileTimers.CreateTimedBlock("User Code"))
#endif
                    {
                        gameObj.SetActive(true);
                    }
                }
            };

            return context.Container;
        }

        protected abstract void AddInstallers(List<TypeValuePair> args, GameObjectContext context);
        protected abstract GameObject CreateGameObject(InjectContext context, out bool shouldMakeActive);
    }
}

#endif
