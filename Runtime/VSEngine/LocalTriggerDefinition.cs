using System;
using Core.Model;
using Core.Model.Stats;
using Core.VSEngine.Nodes;
using Core.VSEngine.Systems;
using FixedPointy;
using UnityEngine;

#nullable enable

namespace Core.VSEngine
{
    [Serializable]
    public sealed class LocalTriggerDefinition
    {
        [SerializeField] public string Name = "";

        public CooldownResetType CooldownReset;
        
        [SerializeField] private StatConfig _CooldownStat;
        public StatConfig CooldownStat => _CooldownStat;

        private static readonly EntId DefaultEntityValue = EntId.Invalid;
        // Tags has no configurable value

        /// <summary>Returns the default value as the runtime type expected by <see cref="NodeElementType"/>.</summary>
        // public object GetDefaultValue()
        // {
        //     // DefaultEntityValue = new EntId((uint)Math.Max(0, EntityIdValue));
        //     return Type switch
        //     {
        //         NodeElementType.Numbers => (Fix) NumberValue,
        //         NodeElementType.Bools => BoolValue,
        //         NodeElementType.Positions => Vector3.zero,
        //         NodeElementType.TileCoordinates => TileCoordValue,
        //         NodeElementType.Tags => new Tag(),
        //         NodeElementType.Entities => DefaultEntityValue,
        //         _ => throw new ArgumentOutOfRangeException(nameof(Type))
        //     };
        // }
    }
}
