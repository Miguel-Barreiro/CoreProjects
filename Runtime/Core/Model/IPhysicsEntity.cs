using UnityEngine;
using Vector2 = System.Numerics.Vector2;

#nullable enable

namespace Core.Model
{
    public interface I2DPhysicsEntity : IComponent
    {
        Vector2 Position { get; set; }
        Rigidbody2D Prefab { get; }
    }
    
}