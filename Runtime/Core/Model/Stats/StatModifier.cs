using System;
using FixedPointy;

namespace Core.Model
{
	internal class StatModifier
	{
		public readonly StatId TargetStatId;
		public readonly StatModId Id; 
            
		public readonly EntId Owner;
		public readonly StatModifierType Type;
		
		private Fix value;
		public Fix Value => value;
		
		public StatModifier(StatModId id, StatModifierType type, Fix initialValue, EntId owner, StatId targetStatId)
		{
			Id = id;
			Type = type;
			value = initialValue;
			Owner = owner;
			TargetStatId = targetStatId;
		}
		
	}
	
	public enum StatModifierType
	{
		None = -1,
		Additive = 0,
		Percentage = 1,
		AdditivePostMultiplicative = 2,
		Multiplicative = 3,
		TOTAL = 4, 
	}
	
	
	[Serializable]
    public struct StatModId : IEquatable<StatModId>
    {
        /// <summary>
        /// This is an indicator of an invalid entity which has no entry in the WorldState.
        /// </summary>
        public static StatModId Invalid = new(int.MinValue);

        public readonly int Id;

        public StatModId(int setId)
        {
            Id = setId;
        }

        public static bool operator ==(StatModId obj1, StatModId obj2)
        {
            return obj1.Id == obj2.Id;
        }

        public static bool operator !=(StatModId obj1, StatModId obj2) => !(obj1 == obj2);

        public bool Equals(StatModId other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is StatModId otherEntId && otherEntId.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            if (IsValid())
            {
                return $"StatModId({Id})";
            }
            else
            {
                return "StatModId(Invalid)";
            }
        }

        public bool IsValid()
        {
            return Id != Invalid.Id;
        }
    }
}