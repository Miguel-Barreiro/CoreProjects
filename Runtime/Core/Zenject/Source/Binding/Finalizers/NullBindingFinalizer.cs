using Core.Zenject.Source.Binding.BindInfo;
using Core.Zenject.Source.Main;
using Zenject;

namespace Core.Zenject.Source.Binding.Finalizers
{
    [NoReflectionBaking]
    public class NullBindingFinalizer : IBindingFinalizer
    {
        public BindingInheritanceMethods BindingInheritanceMethod
        {
            get { return BindingInheritanceMethods.None; }
        }

        public void FinalizeBinding(DiContainer container)
        {
            // Do nothing
        }
    }
}

