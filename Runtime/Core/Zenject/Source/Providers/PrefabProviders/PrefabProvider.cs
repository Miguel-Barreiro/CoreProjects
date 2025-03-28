#if !NOT_UNITY3D

using Core.Zenject.Source.Injection;
using Core.Zenject.Source.Internal;
using Zenject;

namespace Core.Zenject.Source.Providers.PrefabProviders
{
    [NoReflectionBaking]
    public class PrefabProvider : IPrefabProvider
    {
        readonly UnityEngine.Object _prefab;

        public PrefabProvider(UnityEngine.Object prefab)
        {
            Assert.IsNotNull(prefab);
            _prefab = prefab;
        }

        public UnityEngine.Object GetPrefab(InjectContext _)
        {
            return _prefab;
        }
    }
}

#endif


