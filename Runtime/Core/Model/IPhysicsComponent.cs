using Core.Model.ModelSystems;
using UnityEngine;

#nullable enable


namespace Core.Model
{
    public interface IPhysics2DComponent : IPositionComponent, Component<Physics2DComponentData> { }
    
    public struct Physics2DComponentData : IComponentData
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