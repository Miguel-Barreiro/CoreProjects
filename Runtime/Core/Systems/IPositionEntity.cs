using UnityEngine;

namespace Core.Systems
{
    public interface IPositionEntity : IEntity
    {
        Vector2 Position { get; }
        GameObject Prefab { get; }
    }
    
}