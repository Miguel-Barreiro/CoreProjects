using System;
using Cysharp.Threading.Tasks;

#nullable enable

namespace Core.Systems
{
    public interface ILoadSystem
    {
        UniTask<bool> Load(out Action? retryAction);
    }
}