using System;
using Core.Zenject.Source.Binding.Binders.Factory.FactoryFromBinder;
using Core.Zenject.Source.Binding.Binders.Factory.FactoryFromBinder.Untyped;
using Core.Zenject.Source.Binding.BindInfo;
using Core.Zenject.Source.Internal;
using Core.Zenject.Source.Main;
using Zenject;

namespace Core.Zenject.Source.Binding.Binders.Factory.FactoryToChoiceBinder
{
    [NoReflectionBaking]
    public class FactoryToChoiceBinder<TContract> : FactoryFromBinder<TContract>
    {
        public FactoryToChoiceBinder(
            DiContainer container, BindInfo.BindInfo bindInfo, FactoryBindInfo factoryBindInfo)
            : base(container, bindInfo, factoryBindInfo)
        {
        }

        // Note that this is the default, so not necessary to call
        public FactoryFromBinder<TContract> ToSelf()
        {
            Assert.IsEqual(BindInfo.ToChoice, ToChoices.Self);
            return this;
        }

        public FactoryFromBinderUntyped To(Type concreteType)
        {
            BindInfo.ToChoice = ToChoices.Concrete;
            BindInfo.ToTypes.Clear();
            BindInfo.ToTypes.Add(concreteType);

            return new FactoryFromBinderUntyped(
                BindContainer, concreteType, BindInfo, FactoryBindInfo);
        }

        public FactoryFromBinder<TConcrete> To<TConcrete>()
            where TConcrete : TContract
        {
            BindInfo.ToChoice = ToChoices.Concrete;
            BindInfo.ToTypes.Clear();
            BindInfo.ToTypes.Add(typeof(TConcrete));

            return new FactoryFromBinder<TConcrete>(BindContainer, BindInfo, FactoryBindInfo);
        }
    }
}
