using UnityEngine;

#nullable enable


namespace Core.Model
{
    public interface I2DPhysicsEntity : IPositionEntity, Component<I2DPhysicsEntityData>
    {
        // public EntId ID { get; set; }
    }
    
    public struct I2DPhysicsEntityData : IComponentData
    {
        Vector2 Position { get; set; }
        GameObject Prefab { get; set; }
        Rigidbody2D Rigidbody2D { get; set; }
        public EntId ID { get; set; }
    }

}