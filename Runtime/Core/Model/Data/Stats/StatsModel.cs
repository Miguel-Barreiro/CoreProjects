using System;
using System.Collections.Generic;
using System.Linq;
using Core.Model.Stats;
using Core.Utils.CachedDataStructures;
using FixedPointy;
using Debug = UnityEngine.Debug;

namespace Core.Model.Data.Stats
{

	public sealed class StatsModel 
	{
		
		private readonly Dictionary<StatId, Stat> StatsById = new();
		private readonly Dictionary<StatModId, StatModifier> ModifiersById = new();
		
		// private CACHES
		private readonly Dictionary<EntId, Dictionary<StatConfig, StatId>> StatsByOwnerAndType = new();
		private readonly Dictionary<EntId, List<StatModId>> ModifiersByOwner = new();
		
		private uint _statIdGenerator = 0;
		private uint _statModIdGenerator = 0;

		
		public Fix ModifyDepletedValue(EntId targetEntId, StatConfig stat, Fix delta)
		{
			if (!StatsByOwnerAndType.TryGetValue(targetEntId, out Dictionary<StatConfig, StatId> ownerStatsDict))
			{
#if UNITY_EDITOR
				Debug.Log($"StatsModel.ModifyDepletedValue: entity({targetEntId}) not found"); 
#endif
				return stat.DefaultBaseValue;
			}

			if (!ownerStatsDict.TryGetValue(stat, out StatId statId))
			{
				return stat.DefaultBaseValue;
			}

			Stat statData = StatsById[statId];
			// Fix maxValue = CalculateNonDepletedValue(statData);
			
			Fix newDepletedValue = statData.DepletedValue + delta;

			UpdateDepletedAfterStatChange(statData, newDepletedValue);
			// statData.DepletedValue = FixMath.Clamp(newDepletedValue, stat.DefaultMinValue, maxValue);
			return statData.DepletedValue;
		}

		
		public Fix GetDepletedStatPercentage(EntId targetEntId, StatConfig stat)
		{
			if (!StatsByOwnerAndType.TryGetValue(targetEntId, out Dictionary<StatConfig, StatId> ownerStatsDict))
			{
				return Fix.One;
			}

			if (!ownerStatsDict.TryGetValue(stat, out StatId statId))
			{
				return Fix.One;
			}

			Stat statData = StatsById[statId];
			Fix nonDepletedValue = CalculateNonDepletedValue(statData);
			
			if (nonDepletedValue <= 0)
				return Fix.One;
			
			return statData.DepletedValue / nonDepletedValue;
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
		
		
		public void SetBaseValue(EntId targetEntId, StatConfig stat, Fix baseValue, bool resetDepletedValue = false)
		{
			Stat statData = GetOrCreateStat(targetEntId, stat);

			Fix beforeChange = CalculateNonDepletedValue(statData);
			
			statData.BaseValue = baseValue;
			
			Fix afterChange = CalculateNonDepletedValue(statData);

			Fix depletedDelta = afterChange - beforeChange;
			Fix newDepletedValue = statData.DepletedValue + depletedDelta;

			if(resetDepletedValue){
				statData.DepletedValue = afterChange;
			}else
			{
				UpdateDepletedAfterStatChange(statData, newDepletedValue);
			}
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


		
		public void AddPermanentModifier(EntId targetEntId, StatConfig stat, StatModifierType type, Fix value)
		{
			Stat statData = GetOrCreateStat(targetEntId, stat);
			statData.AddPermanentModifier(type, value);
		}

		
		
		public StatModId AddModifier(EntId owner, EntId targetEntId, StatConfig stat, StatModifierType modifierType, Fix modifierValue)
		{
			Stat statData = GetOrCreateStat(targetEntId, stat);
			StatId statId = statData.ID;

			StatModId newStatModID = NextModId();
			StatModifier modifier = new StatModifier(newStatModID, modifierType, modifierValue, owner, statId);


			if (!ModifiersByOwner.TryGetValue(owner, out List<StatModId> ownerModifierList))
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
			UpdateDepletedAfterStatChange(statData, newDepletedValue);
			// statData.DepletedValue = FixMath.Clamp(newDepletedValue, stat.DefaultMinValue, afterChange);
			
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
				UpdateDepletedAfterStatChange(targetStat, newDepletedValue);
				// targetStat.DepletedValue = FixMath.Clamp(newDepletedValue, targetStat.MinValue, afterChange);
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
// #if DEBUG
// 				Debug.LogWarning($"StatsModel.RemoveAllModifiersFrom: owner({modifierOwner}) does not have any modifiers");
// #endif				
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
				
				StatsById.Remove(stat.ID);
				
				// Remove all modifiers affecting each stat
				IEnumerable<StatModId> modIds = stat.GetModifiers();
				foreach (StatModId modId in modIds)
				{
					if (!ModifiersById.TryGetValue(modId, out StatModifier modifier))
					{
						// Debug.LogError($"StatsModel.RemoveAllStatsFrom: modifier {modId} does not exist");
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
			if (!statData.CacheDirty)
			{
				return statData.CachedValue;
			}


			Fix calculatedResult = statData.BaseValue;
			
			// Apply additive modifiers first
			IEnumerable<StatModId> additiveModifiers = statData.GetModifiers(StatModifierType.Additive);
		    foreach ( StatModId modId in additiveModifiers)
		    {
		        calculatedResult += ModifiersById[modId].Value;
		    }
			calculatedResult += statData.GetPermanentModifier(StatModifierType.Additive);
			
			// Apply percentage modifiers
			IEnumerable<StatModId> percentageModifiers = statData.GetModifiers(StatModifierType.Percentage);
			foreach ( StatModId modId in percentageModifiers)
			{
		        calculatedResult += calculatedResult * ModifiersById[modId].Value;
		    }
			calculatedResult += calculatedResult * statData.GetPermanentModifier(StatModifierType.Percentage);
			
			// Apply multiplicative modifiers
			IEnumerable<StatModId> multiplicativeModifiers = statData.GetModifiers(StatModifierType.Multiplicative);
			foreach ( StatModId modId in multiplicativeModifiers)
			{
				calculatedResult *= ModifiersById[modId].Value;
			}
			calculatedResult *= statData.GetPermanentModifier(StatModifierType.Multiplicative);;
			
			// Apply post-multiplicative additive modifiers
			IEnumerable<StatModId> postMultiplicativeModifiers = statData.GetModifiers(StatModifierType.AdditivePostMultiplicative);
			foreach ( StatModId modId in postMultiplicativeModifiers)
			{
				calculatedResult += ModifiersById[modId].Value;
			}
			calculatedResult += statData.GetPermanentModifier(StatModifierType.AdditivePostMultiplicative);

			Fix result = FixMath.Clamp(calculatedResult, statData.MinValue, statData.MaxValue);
			
			statData.CacheDirty = false;
			statData.CachedValue = result;
			
			return result;
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
				statData = new(statId, stat.DefaultBaseValue, stat.DefaultMaxValue, stat.DefaultMinValue, owner, stat.CanOverflow);
				StatsById[statData.ID] = statData;
				ownerStatsDict[stat] = statData.ID;
			} else
			{
				statData = StatsById[statId];
			}
			
			return statData;
		}
		
		private void UpdateDepletedAfterStatChange(Stat statData, Fix newDepletedValue)
		{
			if (!statData.CanOverflow)
			{
				Fix newStatValue = CalculateNonDepletedValue(statData);
				Fix minValue = statData.MinValue;
				statData.DepletedValue = FixMath.Clamp(newDepletedValue, minValue, newStatValue);
			} else
			{
				statData.DepletedValue = newDepletedValue;
			}
		}


		#endregion

		public Fix GetModifierValue(StatModId modId)
		{
			if (!ModifiersById.TryGetValue(modId, out StatModifier modifier))
			{
				// Debug.LogWarning($"StatsModel.GetModifierValue: modifier {modId} does not exist");
				return 0;
			}
			return modifier.Value;
		}

		public void ChangeModifierValue(StatModId modId, Fix newValue)
		{
			if (!ModifiersById.TryGetValue(modId, out StatModifier modifier))
			{
				// Debug.LogWarning($"StatsModel.ChangeModifierValue: modifier {modId} does not exist");
				return;
			}

			Stat statData = StatsById[modifier.TargetStatId];
			Fix beforeChange = CalculateNonDepletedValue(statData);
			
			modifier.Value = newValue;
			statData.CacheDirty = true;
			
			Fix afterChange = CalculateNonDepletedValue(statData);
			
			Fix depletedDelta = afterChange - beforeChange;
			Fix newDepletedValue = statData.DepletedValue + depletedDelta;
			
			UpdateDepletedAfterStatChange(statData, newDepletedValue);
			// statData.DepletedValue = FixMath.Clamp(newDepletedValue, statData.MinValue, afterChange);
		}

		public IEnumerable<StatModId> GetModifiersOwnedBy(EntId owner)
		{
			if (!ModifiersByOwner.TryGetValue(owner, out List<StatModId> modifiers))
			{
				return Enumerable.Empty<StatModId>();
			}
			return modifiers;
		}

		public IEnumerable<StatModId> GetModifiersFromOwnerToTarget(EntId owner, EntId targetEntId)
		{
			if (!ModifiersByOwner.TryGetValue(owner, out List<StatModId> ownerModifiers))
			{
				yield break;
			}

			foreach (StatModId modId in ownerModifiers){
				StatModifier modifier = ModifiersById[modId];
				Stat stat = StatsById[modifier.TargetStatId];
				if (stat == null){
					continue;
				}
				if (stat.Owner == targetEntId){
					yield return modId;
				}
			}
		}

		public Fix SetDepletedValue(EntId targetEntId, StatConfig stat, Fix newDepletedValue)
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
			// Fix maxValue = CalculateNonDepletedValue(statData);
			
			UpdateDepletedAfterStatChange(statData, newDepletedValue);
			// statData.DepletedValue = FixMath.Clamp(newValue, stat.DefaultMinValue, maxValue);
			return statData.DepletedValue;
		}
		
		public void ResetDepletedValueToMax(EntId targetEntId, StatConfig stat)
		{
			if (!StatsByOwnerAndType.TryGetValue(targetEntId, out Dictionary<StatConfig, StatId> ownerStatsDict))
			{
// #if DEBUG
// 				Debug.Log($"StatsModel.ResetDepletedValueToMax: entity({targetEntId}) not found");
// #endif
				return;
			}

			if (!ownerStatsDict.TryGetValue(stat, out StatId statId))
			{
				return;
			}

			Stat statData = StatsById[statId];
			Fix maxValue = CalculateNonDepletedValue(statData);
			
			// Set depleted value to max (full)
			statData.DepletedValue = maxValue;
		}

		public void ResetDepletedValueToMin(EntId targetEntId, StatConfig stat)
		{
			if (!StatsByOwnerAndType.TryGetValue(targetEntId, out Dictionary<StatConfig, StatId> ownerStatsDict))
			{
// #if DEBUG
// 				Debug.Log($"StatsModel.ResetDepletedValueToMin: entity({targetEntId}) not found");
// #endif
				return;
			}

			if (!ownerStatsDict.TryGetValue(stat, out StatId statId))
			{
				return;
			}

			Stat statData = StatsById[statId];
			
			// Set depleted value to minimum
			statData.DepletedValue = stat.DefaultMinValue;
		}

		public void RemoveAllModifiersFrom(EntId owner, EntId targetEntId, StatConfig targetStat)
		{
			if (!ModifiersByOwner.TryGetValue(owner, out List<StatModId> ownerModifiers))
				return;

			if(!StatsByOwnerAndType.TryGetValue(targetEntId, out Dictionary<StatConfig, StatId> ownerStatsDict))
				return;

			if(!ownerStatsDict.TryGetValue(targetStat, out StatId targetStatId))
				return;
			
			using CachedList<StatModId> modifiers = ListCache<StatModId>.Get();
			modifiers.AddRange(ownerModifiers);
			
			foreach (StatModId modId in modifiers){
				StatModifier modifier = ModifiersById[modId];
				Stat targetStatData = StatsById[modifier.TargetStatId];
				if (targetStatData == null)
					continue;
				
				if (targetStatData.Owner == targetEntId && targetStatData.ID == targetStatId)
					RemoveModifier(modId);
			}

			
			
		}

		public void CopyStats(EntId sourceEntityId, EntId targetEntityId)
		{
			// Check if source entity has any stats
			if (!StatsByOwnerAndType.TryGetValue(sourceEntityId, out Dictionary<StatConfig, StatId> sourceStatsDict))
			{
				return; // Source entity has no stats to copy
			}

			// Copy each stat from source to target
			foreach ( (StatConfig statConfig, StatId sourceStatId) in sourceStatsDict)
			{
				// StatConfig statConfig = statConfigAndId.Key;
				// StatId sourceStatId = statConfigAndId.Value;
				Stat sourceStat = StatsById[sourceStatId];

				// Create or get the target stat
				Stat targetStat = GetOrCreateStat(targetEntityId, statConfig);

				// Copy base value and depleted value
				targetStat.BaseValue = sourceStat.BaseValue;
				targetStat.DepletedValue = sourceStat.DepletedValue;

				// Copy permanent modifiers
				for (int i = 0; i < (int)StatModifierType.TOTAL; i++)
				{
					StatModifierType type = (StatModifierType)i;
					Fix permanentValue = sourceStat.GetPermanentModifier(type);
					if (permanentValue != Fix.Zero)
						targetStat.AddPermanentModifier(type, permanentValue);
				}

				IEnumerable<StatModId> statModIds = sourceStat.GetModifiers();
				foreach (StatModId sourceModId in statModIds)
				{
					StatModifier sourceModifier = ModifiersById[sourceModId];
						
					// Create new modifier for target with same owner and value
					StatModId newModId = NextModId();

					StatModifier newModifier = new StatModifier(newModId, sourceModifier.Type, 
																sourceModifier.Value, sourceModifier.Owner, 
																targetStat.ID);

					// Add to ModifiersById
					ModifiersById[newModId] = newModifier;

					// Add to ModifiersByOwner
					if (!ModifiersByOwner.TryGetValue(sourceModifier.Owner, out List<StatModId> ownerModifierList))
					{
						ownerModifierList = new List<StatModId>();
						ModifiersByOwner[sourceModifier.Owner] = ownerModifierList;
					}
					ownerModifierList.Add(newModId);

					// Add to target stat
					targetStat.AddModifier(newModifier);
					
				}
				
			}
		}
	}


    
}