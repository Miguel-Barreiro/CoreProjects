using System;
using Core.Model;
using Core.VSEngine.Nodes;
using FixedPointy;
using UnityEngine;

#nullable enable

namespace Core.VSEngine
{
    [Serializable]
    public class LocalVariableDefinition
    {
        [SerializeField] public string Name = "";
        [SerializeField] public NodeElementType Type = NodeElementType.Numbers;

        // Per-type default value storage — only the field matching Type is used.
        [SerializeField] public float     NumberValue = 0;
        [SerializeField] public bool      BoolValue;
        [SerializeField] public Vector3   PositionValue = Vector3.zero;   // stored as UnityEngine.Vector3 for serialization
        [SerializeField] public Vector2Int TileCoordValue = Vector2Int.zero;

        private static readonly EntId DefaultEntityValue = EntId.Invalid;
        // Tags has no configurable value

        /// <summary>Returns the default value as the runtime type expected by <see cref="NodeElementType"/>.</summary>
        public object GetDefaultValue()
        {
            // DefaultEntityValue = new EntId((uint)Math.Max(0, EntityIdValue));
            return Type switch
            {
                NodeElementType.Numbers => (Fix) NumberValue,
                NodeElementType.Bools => BoolValue,
                NodeElementType.Positions => Vector3.zero,
                NodeElementType.TileCoordinates => TileCoordValue,
                NodeElementType.Tags => new Tag(),
                NodeElementType.Entities => DefaultEntityValue,
                _ => throw new ArgumentOutOfRangeException(nameof(Type))
            };
        }
    }
}
