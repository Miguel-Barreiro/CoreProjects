using Core.Zenject.Source.Internal;
using Core.Zenject.Source.Main;
using Core.Zenject.Source.Runtime.Kernels;
using Core.Zenject.Source.Util;

namespace Core.Zenject.Source.Providers.SubContainerCreators
{
    public static class SubContainerCreatorUtil
    {
        public static void ApplyBindSettings(
            SubContainerCreatorBindInfo subContainerBindInfo, DiContainer subContainer)
        {
            if (subContainerBindInfo.CreateKernel)
            {
                var parentContainer = subContainer.ParentContainers.OnlyOrDefault();
                Assert.IsNotNull(parentContainer, "Could not find unique container when using WithKernel!");

                if (subContainerBindInfo.KernelType != null)
                {
                    parentContainer.Bind(typeof(Kernel).Interfaces()).To(subContainerBindInfo.KernelType)
                        .FromSubContainerResolve()
                        .ByInstance(subContainer).AsCached();
                    subContainer.Bind(subContainerBindInfo.KernelType).AsCached();
                }
                else
                {
                    parentContainer.BindInterfacesTo<Kernel>().FromSubContainerResolve()
                        .ByInstance(subContainer).AsCached();
                    subContainer.Bind<Kernel>().AsCached();
                }

#if !NOT_UNITY3D
                if (subContainerBindInfo.DefaultParentName != null)
                {
                    DefaultGameObjectParentInstaller.Install(
                        subContainer, subContainerBindInfo.DefaultParentName);
                }
#endif
            }
        }
    }
}
