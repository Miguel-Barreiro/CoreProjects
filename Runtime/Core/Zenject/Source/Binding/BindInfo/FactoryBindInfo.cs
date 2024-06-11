using System;
using System.Collections.Generic;
using Core.Zenject.Source.Injection;
using Core.Zenject.Source.Main;
using Core.Zenject.Source.Providers;
using Zenject;

namespace Core.Zenject.Source.Binding.BindInfo
{
    [NoReflectionBaking]
    public class FactoryBindInfo
    {
        public FactoryBindInfo(Type factoryType)
        {
            FactoryType = factoryType;
            Arguments = new List<TypeValuePair>();
        }

        public Type FactoryType
        {
            get; private set;
        }

        public Func<DiContainer, IProvider> ProviderFunc
        {
            get; set;
        }

        public List<TypeValuePair> Arguments
        {
            get;
            set;
        }
    }
}
