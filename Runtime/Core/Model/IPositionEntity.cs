using UnityEngine;

namespace Core.Model
{
    public interface IPositionEntity : IComponent
    {
        Vector2 Position { get; set; }
        GameObject Prefab { get; set; }
    }
    
}