#if !NOT_UNITY3D

using System;
using System.Collections.Generic;
using Core.Zenject.Source.Binding.BindInfo;
using Core.Zenject.Source.Injection;
using UnityEngine;

namespace Core.Zenject.Source.Providers.PrefabCreators
{
    public interface IPrefabInstantiator
    {
        Type ArgumentTarget
        {
            get;
        }

        List<TypeValuePair> ExtraArguments
        {
            get;
        }

        GameObjectCreationParameters GameObjectCreationParameters
        {
            get;
        }

        GameObject Instantiate(InjectContext context, List<TypeValuePair> args, out Action injectAction);

        UnityEngine.Object GetPrefab(InjectContext context);
    }
}

#endif
