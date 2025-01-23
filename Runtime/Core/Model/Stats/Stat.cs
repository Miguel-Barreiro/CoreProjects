using System;
using System.Collections.Generic;
using FixedPointy;

namespace Core.Model
{
	internal class Stat
    {
        public readonly StatId Id;
        
        private readonly List<StatModId>[] Modifiers;
        public Fix BaseValue;
        public Fix DepletedValue;
        
        
        
        public Stat(StatId id, Fix baseValue)
        {
            Id = id;
            Modifiers = new List<StatModId>[(int) StatModifierType.TOTAL];
            BaseValue = baseValue;
            DepletedValue = baseValue;
        }
        
        
        public void AddModifier(StatModifier modifier)
        {
            int index = (int)modifier.Type;
            if (Modifiers[index] == null)
            {
                Modifiers[index] = new List<StatModId>();
            } else if( Modifiers[index].Contains(modifier.Id))
            {
                return;
            }
            Modifiers[index].Add(modifier.Id);
        }

        public void RemoveModifier(StatModifier modifier)
        {
            int index = (int)modifier.Type;
            if (Modifiers[index] == null)
            {
                return;
            }
            Modifiers[index].Remove(modifier.Id);
        }

        public IEnumerable<StatModId> GetModifiers(StatModifierType type)
        {
            int index = (int) type;
            if (Modifiers[index] != null)
            {
                foreach (StatModId statModId in Modifiers[index])
                {
                    yield return statModId;
                }
            }
        }

        public IEnumerable<StatModId> GetModifiers()
        {
            foreach (List<StatModId> modifierList in Modifiers)
            {
                foreach (StatModId statModId in modifierList)
                {
                    yield return statModId;
                }
            }
        }
        
    }
	
	[Serializable]
    public struct StatId : IEquatable<StatId>
    {
        /// <summary>
        /// This is an indicator of an invalid entity which has no entry in the WorldState.
        /// </summary>
        public static StatId Invalid = new(int.MinValue);

        public readonly int Id;

        public StatId(int setId)
        {
            Id = setId;
        }

        public static bool operator ==(StatId obj1, StatId obj2)
        {
            return obj1.Id == obj2.Id;
        }

        public static bool operator !=(StatId obj1, StatId obj2) => !(obj1 == obj2);

        public bool Equals(StatId other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is StatId otherEntId && otherEntId.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            if (IsValid())
            {
                return $"StatId({Id})";
            }
            else
            {
                return "StatId(Invalid)";
            }
        }

        public bool IsValid()
        {
            return Id != Invalid.Id;
        }
    }
}