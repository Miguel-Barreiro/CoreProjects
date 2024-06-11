using System.Linq;
using Core.Zenject.Source.Binding.BindInfo;
using Core.Zenject.Source.Binding.Finalizers;
using Core.Zenject.Source.Factories;
using Core.Zenject.Source.Factories.Pooling;
using Core.Zenject.Source.Injection;
using Core.Zenject.Source.Internal;
using Core.Zenject.Source.Main;
using Core.Zenject.Source.Providers;
using Zenject;

namespace Core.Zenject.Source.Binding.Binders.Factory.Pooling
{
    [NoReflectionBaking]
    public class MemoryPoolBindingFinalizer<TContract> : ProviderBindingFinalizer
    {
        readonly MemoryPoolBindInfo _poolBindInfo;
        readonly FactoryBindInfo _factoryBindInfo;

        public MemoryPoolBindingFinalizer(
            BindInfo.BindInfo bindInfo, FactoryBindInfo factoryBindInfo, MemoryPoolBindInfo poolBindInfo)
            : base(bindInfo)
        {
            // Note that it doesn't derive from MemoryPool<TContract>
            // when used with To<>, so we can only check IMemoryPoolBase
            Assert.That(factoryBindInfo.FactoryType.DerivesFrom<IMemoryPool>());

            _factoryBindInfo = factoryBindInfo;
            _poolBindInfo = poolBindInfo;
        }

        protected override void OnFinalizeBinding(DiContainer container)
        {
            var factory = new FactoryProviderWrapper<TContract>(
                _factoryBindInfo.ProviderFunc(container), new InjectContext(container, typeof(TContract)));

            var settings = new MemoryPoolSettings(
                _poolBindInfo.InitialSize, _poolBindInfo.MaxSize, _poolBindInfo.ExpandMethod);

            var transientProvider = new TransientProvider(
                _factoryBindInfo.FactoryType,
                container,
                _factoryBindInfo.Arguments.Concat(
                    InjectUtil.CreateArgListExplicit(factory, settings)).ToList(),
                BindInfo.ContextInfo, BindInfo.ConcreteIdentifier, null);

            IProvider mainProvider;

            if (BindInfo.Scope == ScopeTypes.Unset || BindInfo.Scope == ScopeTypes.Singleton)
            {
                mainProvider = BindingUtil.CreateCachedProvider(transientProvider);
            }
            else
            {
                Assert.IsEqual(BindInfo.Scope, ScopeTypes.Transient);
                mainProvider = transientProvider;
            }

            RegisterProviderForAllContracts(container, mainProvider);
        }
    }
}

