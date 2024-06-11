using Core.Zenject.Source.Injection;

#if !NOT_UNITY3D

namespace Core.Zenject.Source.Providers.PrefabProviders
{
    public interface IPrefabProvider
    {
        UnityEngine.Object GetPrefab(InjectContext context);
    }
}

#endif

