#if !NOT_UNITY3D

using System;
using System.Collections.Generic;
using Core.Zenject.Source.Binding.BindInfo;
using Core.Zenject.Source.Injection;
using Core.Zenject.Source.Internal;
using Core.Zenject.Source.Main;
using UnityEngine;
using Zenject;

namespace Core.Zenject.Source.Providers.GameObjectProviders
{
    [NoReflectionBaking]
    public class EmptyGameObjectProvider : IProvider
    {
        readonly DiContainer _container;
        readonly GameObjectCreationParameters _gameObjectBindInfo;

        public EmptyGameObjectProvider(
            DiContainer container, GameObjectCreationParameters gameObjectBindInfo)
        {
            _gameObjectBindInfo = gameObjectBindInfo;
            _container = container;
        }

        public bool IsCached
        {
            get { return false; }
        }

        public bool TypeVariesBasedOnMemberType
        {
            get { return false; }
        }

        public Type GetInstanceType(InjectContext context)
        {
            return typeof(GameObject);
        }

        public void GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args, out Action injectAction, List<object> buffer)
        {
            Assert.IsEmpty(args);

            injectAction = null;

            var gameObj = _container.CreateEmptyGameObject(_gameObjectBindInfo, context);
            buffer.Add(gameObj);
        }
    }
}

#endif

