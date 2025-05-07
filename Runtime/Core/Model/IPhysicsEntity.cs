using Core.Model.ModelSystems;
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
        public Vector2 Position { get; set; }
        public GameObject Prefab { get; set; }
        public Rigidbody2D Rigidbody2D { get; set; }
        public EntId ID { get; set; }
    }

}