#nullable enable

using System;
using Cysharp.Threading.Tasks;

namespace Core.Systems
{
    public interface ILoadSystem
    {
        UniTask<bool> Load(out Action? retryAction);
    }
}