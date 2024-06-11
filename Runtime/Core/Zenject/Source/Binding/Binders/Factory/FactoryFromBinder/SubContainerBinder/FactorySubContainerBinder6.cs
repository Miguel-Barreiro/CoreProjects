using System;
using Core.Zenject.Source.Binding.Binders.GameObject;
using Core.Zenject.Source.Binding.BindInfo;
using Core.Zenject.Source.Injection;
using Core.Zenject.Source.Main;
using Core.Zenject.Source.Providers;
using Core.Zenject.Source.Providers.PrefabProviders;
using Core.Zenject.Source.Providers.SubContainerCreators;
using Zenject;

namespace Core.Zenject.Source.Binding.Binders.Factory.FactoryFromBinder.SubContainerBinder
{
    [NoReflectionBaking]
    public class FactorySubContainerBinder<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TContract>
        : FactorySubContainerBinderWithParams<TContract>
    {
        public FactorySubContainerBinder(
            DiContainer bindContainer, BindInfo.BindInfo bindInfo, FactoryBindInfo factoryBindInfo, object subIdentifier)
            : base(bindContainer, bindInfo, factoryBindInfo, subIdentifier)
        {
        }

        public ScopeConcreteIdArgConditionCopyNonLazyBinder ByMethod(
#if !NET_4_6
            Internal.
#endif
            Action<DiContainer, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> installerMethod)
        {
            var subcontainerBindInfo = new SubContainerCreatorBindInfo();

            ProviderFunc =
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByMethod<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
                        container, subcontainerBindInfo, installerMethod), false);

            return new ScopeConcreteIdArgConditionCopyNonLazyBinder(BindInfo);
        }

#if !NOT_UNITY3D
        public NameTransformScopeConcreteIdArgConditionCopyNonLazyBinder ByNewGameObjectMethod(
#if !NET_4_6
            Internal.
#endif
            Action<DiContainer, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> installerMethod)
        {
            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByNewGameObjectMethod<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
                        container, gameObjectInfo, installerMethod), false);

            return new NameTransformScopeConcreteIdArgConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformScopeConcreteIdArgConditionCopyNonLazyBinder ByNewPrefabMethod(
            Func<InjectContext, UnityEngine.Object> prefabGetter,
#if !NET_4_6
            Internal.
#endif
            Action<DiContainer, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> installerMethod)
        {
            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByNewPrefabMethod<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
                        container,
                        new PrefabProviderCustom(prefabGetter),
                        gameObjectInfo, installerMethod), false);

            return new NameTransformScopeConcreteIdArgConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformScopeConcreteIdArgConditionCopyNonLazyBinder ByNewPrefabMethod(
            UnityEngine.Object prefab,
#if !NET_4_6
            Internal.
#endif
            Action<DiContainer, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> installerMethod)
        {
            BindingUtil.AssertIsValidPrefab(prefab);

            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByNewPrefabMethod<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
                        container,
                        new PrefabProvider(prefab),
                        gameObjectInfo, installerMethod), false);

            return new NameTransformScopeConcreteIdArgConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformScopeConcreteIdArgConditionCopyNonLazyBinder ByNewPrefabResourceMethod(
            string resourcePath,
#if !NET_4_6
            Internal.
#endif
            Action<DiContainer, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> installerMethod)
        {
            BindingUtil.AssertIsValidResourcePath(resourcePath);

            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new SubContainerDependencyProvider(
                    ContractType, SubIdentifier,
                    new SubContainerCreatorByNewPrefabMethod<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6>(
                        container,
                        new PrefabProviderResource(resourcePath),
                        gameObjectInfo, installerMethod), false);

            return new NameTransformScopeConcreteIdArgConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }
#endif
    }
}
