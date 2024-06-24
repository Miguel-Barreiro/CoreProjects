using Core.Model;
using UnityEngine;

namespace Core.Systems
{
    public interface IPositionEntity : IComponent
    {
        Vector2 Position { get; }
        GameObject Prefab { get; }
    }
    
}