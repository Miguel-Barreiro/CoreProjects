using Core.Model.ModelSystems;
using UnityEngine;

#nullable enable


namespace Core.Model
{
    public interface PhysicsComponent2D : IPositionComponent, Component<PhysicsEntity2DData>
    {
        // public EntId ID { get; set; }
    }
    
    public struct PhysicsEntity2DData : IComponentData
    {
        public Vector2 Position;
        public GameObject Prefab;
        public Rigidbody2D Rigidbody2D;
        public EntId ID { get; set; }

        public void Init()
        {
            Prefab = null;
            Rigidbody2D = null;
            Position = Vector2.zero;
        }
    }

}