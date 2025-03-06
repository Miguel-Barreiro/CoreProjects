using UnityEngine;

#nullable enable


namespace Core.Model
{
    public interface I2DPhysicsEntity : IPositionEntity
    {
        Vector2 Position { get; set; }
        GameObject Prefab { get; set; }
        Rigidbody2D Rigidbody2D { get; set; }
    }
    
}