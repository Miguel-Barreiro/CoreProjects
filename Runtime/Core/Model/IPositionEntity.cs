using UnityEngine;

namespace Core.Model
{
    public struct PositionEntity : IComponentData
    {
        Vector2 Position { get; set; }

        public EntId ID { get; set; }
    }
    
    public interface IPositionEntity : Component<PositionEntity> { }
    
}