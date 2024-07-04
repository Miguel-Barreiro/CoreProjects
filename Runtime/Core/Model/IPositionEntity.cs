using UnityEngine;

namespace Core.Model
{
    public interface IPositionEntity : IComponent
    {
        Vector2 Position { get; }
        GameObject Prefab { get; }
    }
    
}