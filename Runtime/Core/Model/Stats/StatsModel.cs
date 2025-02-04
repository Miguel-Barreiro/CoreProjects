using System.Diagnostics;
using System.Collections.Generic;
using FixedPointy;
using Debug = UnityEngine.Debug;

namespace Core.Model
{
    public interface IStatsEntity : IComponent
    {
        // Marker interface for entities that can have stats
    }

	public sealed class StatsModel : BaseEntity
	{
		
		private readonly Dictionary<StatId, Stat> StatsById = new();
		private readonly Dictionary<StatModId, StatModifier> ModifiersById = new();
		
		// private CACHES
		private readonly Dictionary<EntId, Dictionary<StatConfig, StatId>> StatsByOwnerAndType = new();
		private readonly Dictionary<EntId, List<StatModId>> ModifiersByOwner = new();
		
		private int _statIdGenerator = 0;
		private int _statModIdGenerator = 0;

		
		public void ModifyDepletedValue(EntId targetEntId, StatConfig stat, Fix delta)
		{
			if (!StatsByOwnerAndType.TryGetValue(targetEntId, out Dictionary<StatConfig, StatId> ownerStatsDict))
			{
#if DEBUG
				Debug.Log($"StatsModel.ModifyDepletedValue: entity({targetEntId}) not found"); 
#endif				
				return;
			}

			if (!ownerStatsDict.TryGetValue(stat, out StatId statId))
			{
				return;
			}

			Stat statData = StatsById[statId];
			Fix maxValue = CalculateNonDepletedValue(statData);
			
			Fix newDepletedValue = statData.DepletedValue + delta;
			
			statData.DepletedValue = FixMath.Clamp(newDepletedValue, stat.DefaultMinValue, maxValue);
		}

		public Fix GetDepletedStatValue(EntId targetEntId, StatConfig stat)
		{
			if (!StatsByOwnerAndType.TryGetValue(targetEntId, out Dictionary<StatConfig, StatId> ownerStatsDict))
			{
				return stat.DefaultBaseValue;
			}

			if (!ownerStatsDict.TryGetValue(stat, out StatId statId))
			{
				return stat.DefaultBaseValue;
			}

			Stat statData = StatsById[statId];
			return statData.DepletedValue;
		}
		
		
		public void SetBaseValue(EntId targetEntId, StatConfig stat, Fix baseValue)
		{
			Stat statData = GetOrCreateStat(targetEntId, stat);

			Fix beforeChange = CalculateNonDepletedValue(statData);
			
			statData.BaseValue = baseValue;
			
			Fix afterChange = CalculateNonDepletedValue(statData);

			Fix depletedDelta = afterChange - beforeChange;
			Fix newDepletedValue = statData.DepletedValue + depletedDelta;
			statData.DepletedValue = FixMath.Clamp(newDepletedValue, stat.DefaultMinValue, afterChange);
		}

		public Fix GetStatValue(EntId targetEntId, StatConfig stat)
		{
			if (!StatsByOwnerAndType.TryGetValue(targetEntId, out Dictionary<StatConfig,StatId> statsDict))
			{
				return stat.DefaultBaseValue;
			}

			if (!statsDict.TryGetValue(stat, out StatId statId))
			{
				return stat.DefaultBaseValue;
			}

			Stat statData = StatsById[statId];

			return CalculateNonDepletedValue(statData);
		}


		
		
		public StatModId AddModifier(EntId owner, EntId targetEntId, StatConfig stat, StatModifierType modifierType, Fix modifierValue)
		{
			Stat statData = GetOrCreateStat(targetEntId, stat);
			StatId statId = statData.Id;

			StatModId newStatModID = NextModId();
			StatModifier modifier = new StatModifier(newStatModID, modifierType, modifierValue, owner, statId);


			List<StatModId> ownerModifierList;
			if (!ModifiersByOwner.TryGetValue(owner, out ownerModifierList))
		    {
				ownerModifierList = new List<StatModId>();
		        ModifiersByOwner[owner] = ownerModifierList;
		    }
			
			ownerModifierList.Add(modifier.Id);
		    ModifiersById[modifier.Id] = modifier;
			
			Fix beforeChange = CalculateNonDepletedValue(statData);
			
			statData.AddModifier(modifier);
			
			Fix afterChange = CalculateNonDepletedValue(statData);

			Fix depletedDelta = afterChange - beforeChange;
			Fix newDepletedValue = statData.DepletedValue + depletedDelta;
			statData.DepletedValue = FixMath.Clamp(newDepletedValue, stat.DefaultMinValue, afterChange);
			
			return newStatModID;
		}
		
		
		public void RemoveModifier(StatModId modId)
		{
			StatModifier modifier;
			if (!ModifiersById.TryGetValue(modId, out modifier))
		    {
				Debug.LogError($"StatsModel.RemoveModifier: modifier {modId} does not exist");
		        return;
		    }
		
			
		    // Remove from Stat's modifiers
			if (StatsById.TryGetValue(modifier.TargetStatId, out Stat targetStat))
		    {
				Fix beforeChange = CalculateNonDepletedValue(targetStat);

				targetStat.RemoveModifier(modifier);
				
				Fix afterChange = CalculateNonDepletedValue(targetStat);

				Fix depletedDelta = afterChange - beforeChange;
				Fix newDepletedValue = targetStat.DepletedValue + depletedDelta;
				targetStat.DepletedValue = FixMath.Clamp(newDepletedValue, targetStat.MinValue, afterChange);
		    }

			// Remove from ModifiersByOwner
			if (ModifiersByOwner.TryGetValue(modifier.Owner, out List<StatModId> ownerModifiers))
		    {
		        ownerModifiers.Remove(modId);
		    }

			// Remove from ModifiersById
		    ModifiersById.Remove(modId);
		}
		
		
		
		public void RemoveAllModifiersFrom(EntId modifierOwner)
		{
		    if (!ModifiersByOwner.TryGetValue(modifierOwner, out List<StatModId> modifiers))
		    {
#if DEBUG
				Debug.LogWarning($"StatsModel.RemoveAllModifiersFrom: owner({modifierOwner}) does not have any modifiers");
#endif				
		        return;
		    }
		
		    foreach (StatModId modifierId in modifiers)
			{
				StatModifier modifier = ModifiersById[modifierId];
				ModifiersById.Remove(modifierId);
				
		        Stat stat = StatsById[modifier.TargetStatId];
		        stat.RemoveModifier(modifier);
			}
			
		    ModifiersByOwner.Remove(modifierOwner);
		}
		
		public void RemoveAllStatsFrom(EntId target)
	    {
			if (!StatsByOwnerAndType.TryGetValue(target, out Dictionary<StatConfig, StatId> ownerStatsDict))
			{
				return;
			}

            foreach (StatId statId in ownerStatsDict.Values)
            {
				Stat stat = StatsById[statId];
				
				StatsById.Remove(stat.Id);
				
				// Remove all modifiers affecting each stat
				IEnumerable<StatModId> modIds = stat.GetModifiers();
				foreach (StatModId modId in modIds)
				{
					if (!ModifiersById.TryGetValue(modId, out StatModifier modifier))
					{
						Debug.LogError($"StatsModel.RemoveAllStatsFrom: modifier {modId} does not exist");
						continue;
					}
		
					// Remove from ModifiersById
					ModifiersById.Remove(modId);
		    
					// Remove from ModifiersByOwner
					if (ModifiersByOwner.TryGetValue(modifier.Owner, out List<StatModId> ownerModifiers))
					{
						ownerModifiers.Remove(modId);
					}
                }
            }
			
            StatsByOwnerAndType.Remove(target);
	    }
		
		

		#region Internal

		private StatModId NextModId()
		{
			return new StatModId(_statModIdGenerator++);
		}

		private StatId NextStatId()
		{
			return new StatId(_statIdGenerator++);
		}

		
		
		private Fix CalculateNonDepletedValue(Stat statData)
		{
			Fix calculatedResult = statData.BaseValue;
			
			// Apply additive modifiers first
			IEnumerable<StatModId> additiveModifiers = statData.GetModifiers(StatModifierType.Additive);
		    foreach ( StatModId modId in additiveModifiers)
		    {
		        calculatedResult += ModifiersById[modId].Value;
		    }
			
			// Apply percentage modifiers
			IEnumerable<StatModId> percentageModifiers = statData.GetModifiers(StatModifierType.Percentage);
			foreach ( StatModId modId in percentageModifiers)
			{
		        calculatedResult += calculatedResult * ModifiersById[modId].Value;
		    }
			
			// Apply multiplicative modifiers
			IEnumerable<StatModId> multiplicativeModifiers = statData.GetModifiers(StatModifierType.Multiplicative);
			foreach ( StatModId modId in multiplicativeModifiers)
			{
				calculatedResult *= ModifiersById[modId].Value;
			}
			
			// Apply post-multiplicative additive modifiers
			IEnumerable<StatModId> postMultiplicativeModifiers = statData.GetModifiers(StatModifierType.AdditivePostMultiplicative);
			foreach ( StatModId modId in postMultiplicativeModifiers)
			{
				calculatedResult += ModifiersById[modId].Value;
			}

			return FixMath.Clamp(calculatedResult, statData.MinValue, statData.MaxValue);
		}
		
		private Stat GetOrCreateStat(EntId owner, StatConfig stat)
		{
			Stat statData;
			
			if (!StatsByOwnerAndType.TryGetValue(owner, out Dictionary<StatConfig,StatId> ownerStatsDict))
			{
				ownerStatsDict = new Dictionary<StatConfig, StatId>();
				StatsByOwnerAndType[owner] = ownerStatsDict;
			}
			
			if (!ownerStatsDict.TryGetValue(stat, out StatId statId))
			{
				statId = NextStatId();
				statData = new( statId, stat.DefaultBaseValue, stat.DefaultMaxValue, stat.DefaultMinValue);
				StatsById[statData.Id] = statData;
				ownerStatsDict[stat] = statData.Id;
			} else
			{
				statData = StatsById[statId];
			}
			
			return statData;
		}
		

		#endregion


	}


    
}