using Core.Zenject.Source.Binding.Binders.Factory.FactoryFromBinder;
using Core.Zenject.Source.Binding.BindInfo;
using Core.Zenject.Source.Main;
using Zenject;

namespace Core.Zenject.Source.Binding.Binders.Factory.FactoryToChoiceBinder
{
    [NoReflectionBaking]
    public class DecoratorToChoiceFromBinder<TContract>
    {
        DiContainer _bindContainer;
        BindInfo.BindInfo _bindInfo;
        FactoryBindInfo _factoryBindInfo;

        public DecoratorToChoiceFromBinder(
            DiContainer bindContainer, BindInfo.BindInfo bindInfo, FactoryBindInfo factoryBindInfo)
        {
            _bindContainer = bindContainer;
            _bindInfo = bindInfo;
            _factoryBindInfo = factoryBindInfo;
        }

        public FactoryFromBinder<TContract, TConcrete> With<TConcrete>()
            where TConcrete : TContract
        {
            _bindInfo.ToChoice = ToChoices.Concrete;
            _bindInfo.ToTypes.Clear();
            _bindInfo.ToTypes.Add(typeof(TConcrete));

            return new FactoryFromBinder<TContract, TConcrete>(
                _bindContainer, _bindInfo, _factoryBindInfo);
        }
    }
}
