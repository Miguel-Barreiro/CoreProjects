using Core.Model.ModelSystems;
using UnityEngine;

namespace Core.Model
{
    public struct PositionComponentData : IComponentData
    {
        public Vector3 Position;

        public EntId ID { get; set; }

        public void Init()
        {
            Position = Vector2.zero;
        }
    }
    
    public interface IPositionComponent : Component<PositionComponentData> { }
    
}