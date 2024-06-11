using System;
using Core.Zenject.Source.Binding.BindInfo;
using Core.Zenject.Source.Internal;
using Core.Zenject.Source.Main;
using Core.Zenject.Source.Providers;
using Zenject;

namespace Core.Zenject.Source.Binding.Finalizers
{
    [NoReflectionBaking]
    public class SingleProviderBindingFinalizer : ProviderBindingFinalizer
    {
        readonly Func<DiContainer, Type, IProvider> _providerFactory;

        public SingleProviderBindingFinalizer(
            BindInfo.BindInfo bindInfo, Func<DiContainer, Type, IProvider> providerFactory)
            : base(bindInfo)
        {
            _providerFactory = providerFactory;
        }

        protected override void OnFinalizeBinding(DiContainer container)
        {
            if (BindInfo.ToChoice == ToChoices.Self)
            {
                Assert.IsEmpty(BindInfo.ToTypes);

                RegisterProviderPerContract(container, _providerFactory);
            }
            else
            {
                // Empty sometimes when using convention based bindings
                if (!BindInfo.ToTypes.IsEmpty())
                {
                    RegisterProvidersForAllContractsPerConcreteType(
                        container, BindInfo.ToTypes, _providerFactory);
                }
            }
        }
    }
}
