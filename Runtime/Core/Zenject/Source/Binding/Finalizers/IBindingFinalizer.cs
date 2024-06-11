using Core.Zenject.Source.Binding.BindInfo;
using Core.Zenject.Source.Main;

namespace Core.Zenject.Source.Binding.Finalizers
{
    public interface IBindingFinalizer
    {
        BindingInheritanceMethods BindingInheritanceMethod
        {
            get;
        }

        void FinalizeBinding(DiContainer container);
    }
}
