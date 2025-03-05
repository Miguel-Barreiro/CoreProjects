using UnityEngine;

#nullable enable


namespace Core.Model
{
    public interface I2DPhysicsEntity : IPositionEntity
    {
        Rigidbody2D Rigidbody2D { get; set; }
    }
    
}