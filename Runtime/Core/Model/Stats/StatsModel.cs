using System.Collections.Generic;
using FixedPointy;

namespace Core.Model
{
	public interface IStatsEntity : IComponent
	{
		
	}

	public sealed class StatsModel : BaseEntity
	{

		
		// public Fix GetStatValue(EntId targetEntId, StatConfig stat)
		// {
		// 	
		// }
		//
		// public Fix GetMaxStatValue(EntId targetEntId, StatConfig stat)
		// {
		// 	
		// }
		//
		// public Fix GetMinStatValue(EntId targetEntId, StatConfig stat)
		// {
		// 	
		// }
		//
		// public void AddModifier(EntId owner, EntId targetEntId, StatConfig stat, StatModifier statModifier, Fix modifierValue)
		// {
		// 	
		// }
		//
		// public void RemoveAllModifiersFrom(EntId modifierOwner)
		// {
		// 	
		// }
		//
		// public void RemoveAllStatsFrom(EntId target)
		// {
		// 	
		// }
		//
		private readonly Dictionary<EntId, List<Modifier>> ModifiersByOwner = new();
		private readonly Dictionary<EntId, Dictionary<StatConfig, Stat>> StatsByOwnerAndType = new();
		private readonly int StatIdGenerator = 0;

		private struct Modifier
		{
			public int StatId;
			public EntId Owner;
			public Fix Value;
			public StatModifierType Type;
		}
		
		private class Stat
		{
			public int StatId;
			public readonly List<Modifier>[] Modifiers;
			public Fix BaseValue;
			public Fix DepletedValue;

			public Stat(int statId, Fix baseValue)
			{
				StatId = statId;
				Modifiers = new List<Modifier>[TOTAL_TYPES_MODIFIER];
			}

			public void AddModifier(Modifier modifier, Fix value)
			{
				int index = (int)modifier.Type;
				if (Modifiers[index] == null)
				{
					Modifiers[index] = new List<Modifier>();
				}

				Modifiers[index].Add(modifier);
			}
			
			// public void Update

			public Fix CalculateNonDepletedValue()
			{
				Fix calculatedResult = BaseValue;

				List<Modifier>  modifiers = Modifiers[(int) StatModifierType.Additive];
				if (modifiers != null)
				{
					foreach (Modifier modifier in modifiers)
					{
						calculatedResult += modifier.Value;
					}
				}
				modifiers = Modifiers[(int) StatModifierType.Percentage];
				if (modifiers != null)
				{
					foreach (Modifier modifier in modifiers)
					{
						calculatedResult += calculatedResult * (1+modifier.Value);
					}
				}
				modifiers = Modifiers[(int) StatModifierType.Multiplicative];
				if (modifiers != null)
				{
					foreach (Modifier modifier in modifiers)
					{
						calculatedResult *= modifier.Value;
					}
				}
				modifiers = Modifiers[(int) StatModifierType.AdditivePostMultiplicative];
				if (modifiers != null)
				{
					foreach (Modifier modifier in modifiers)
					{
						calculatedResult += modifier.Value;
					}
				}
				

				return calculatedResult;
			}
		}

		private const int TOTAL_TYPES_MODIFIER = 4;

	}



	public enum StatModifierType
	{
		None = 0,
		Additive = 1,
		Percentage = 2,
		Multiplicative = 4,
		AdditivePostMultiplicative = 3,
	}

}