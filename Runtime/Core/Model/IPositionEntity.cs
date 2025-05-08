using Core.Model.ModelSystems;
using UnityEngine;

namespace Core.Model
{
    public struct PositionEntity : IComponentData
    {
        public Vector2 Position;

        public EntId ID { get; set; }

        public void Init()
        {
            Position = Vector2.zero;
        }
    }
    
    public interface IPositionEntity : Component<PositionEntity> { }
    
}