using Core.Zenject.Source.Binding.Binders.Factory.FactoryArgumentsToChoiceBinder;
using Core.Zenject.Source.Binding.BindInfo;
using Core.Zenject.Source.Main;
using Zenject;

namespace Core.Zenject.Source.Binding.Binders.Factory.FactoryToChoiceIdBinder
{
    [NoReflectionBaking]
    public class FactoryToChoiceIdBinder<TContract> : FactoryArgumentsToChoiceBinder<TContract>
    {
        public FactoryToChoiceIdBinder(
            DiContainer container, BindInfo.BindInfo bindInfo, FactoryBindInfo factoryBindInfo)
            : base(container, bindInfo, factoryBindInfo)
        {
        }

        public FactoryArgumentsToChoiceBinder<TContract> WithId(object identifier)
        {
            BindInfo.Identifier = identifier;
            return this;
        }
    }
}


