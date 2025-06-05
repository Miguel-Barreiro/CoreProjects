using System.Collections.Generic;
using Core.Model.Data.Stats;
using Core.Model.ModelSystems;
using Core.Model.Stats;
using Core.Systems;
using FixedPointy;

namespace Core.Model
{

	public interface StatsSystemRo
	{
		public Fix GetStatValue(EntId targetEntId, StatConfig stat);
		public Fix GetStatDepletedValue(EntId targetEntId, StatConfig stat);
		public Fix GetModifierValue(StatModId statModId);
		
		public IEnumerable<StatModId> GetModifiersOwnedBy(EntId owner);
		public IEnumerable<StatModId> GetModifiers(EntId owner, EntId targetEntId);

	}
	
	
	// public interface IStatsComponent : Component<StatsComponentData> { }
	//
	// public struct StatsComponentData : IComponentData
	// {
	// 	public EntId ID { get; set; }
	// 	public void Init() { }
	// }
	

	public interface StatsSystem : StatsSystemRo
	{

		public void SetBaseValue(EntId targetEntId, StatConfig stat, Fix newValue, bool resetDepletedValue = false);
		public Fix ChangeDepletedValue(EntId targetEntId, StatConfig stat, Fix delta);

		public Fix SetDepletedValue(EntId targetEntId, StatConfig stat, Fix newValue);
		public void ResetDepletedValueToMax(EntId targetEntId, StatConfig stat);
		public void ResetDepletedValueToMin(EntId targetEntId, StatConfig stat);
		
		
		public Fix GetStatValue(EntId targetEntId, StatConfig stat);
		public Fix GetStatDepletedValue(EntId targetEntId, StatConfig stat);
		public Fix GetStatDepletedPercentage(EntId targetEntId, StatConfig stat);
		
		public void AddPermanentModifier(EntId targetEntId, StatConfig stat, Fix value, StatModifierType type);
		
		public StatModId AddModifier(EntId owner, EntId targetEntId, StatConfig stat, Fix value, StatModifierType type);
		public Fix GetModifierValue(StatModId statModId);


		public void RemoveAllModifiersFrom(EntId owner, EntId targetEntId, StatConfig stat);
		
		public void ChangeModifier(StatModId statModId, Fix newValue);
		public IEnumerable<StatModId> GetModifiersOwnedBy(EntId owner);
		public IEnumerable<StatModId> GetModifiers(EntId owner, EntId targetEntId);

		public void RemoveModifier(StatModId statModId);

		public void Reset();
	}
	


	public class StatsSystemImplementation : StatsSystem, StatsSystemRo, IInitSystem, 
											IOnDestroyEntitySystem
	{
		private StatsModel _statsModel = null;
		
		public void Initialize()
		{
			if(_statsModel == null)
				_statsModel = new StatsModel();
		}

		public void OnDestroyEntity(EntId destroyedEntityId)
		{
			_statsModel.RemoveAllModifiersFrom(destroyedEntityId); 
			_statsModel.RemoveAllStatsFrom(destroyedEntityId);
			
		}


		public bool Active { get; set; } = true;
		public SystemGroup Group => CoreSystemGroups.CoreSystemGroup;


		public void Reset()
		{
			_statsModel = new StatsModel();
		}

		public void SetBaseValue(EntId targetEntId, StatConfig stat, Fix newValue, bool resetDepletedValue = false)
		{
			_statsModel.SetBaseValue(targetEntId, stat, newValue, resetDepletedValue);
		}

		public Fix ChangeDepletedValue(EntId targetEntId, StatConfig stat, Fix delta)
		{
			return _statsModel.ModifyDepletedValue(targetEntId, stat, delta);
		}

		public Fix GetStatValue(EntId targetEntId, StatConfig stat)
		{
			return _statsModel.GetStatValue(targetEntId, stat);
		}

		public Fix GetStatDepletedValue(EntId targetEntId, StatConfig stat)
		{
			return _statsModel.GetDepletedStatValue(targetEntId, stat);
		}

		public Fix GetStatDepletedPercentage(EntId targetEntId, StatConfig stat)
		{
			return _statsModel.GetDepletedStatPercentage(targetEntId, stat);
		}

		public StatModId AddModifier(EntId owner, EntId targetEntId, StatConfig stat, Fix value, StatModifierType type)
		{
			return _statsModel.AddModifier(owner, targetEntId, stat, type, value);
		}

		public void AddPermanentModifier(EntId targetEntId, StatConfig stat, Fix value, StatModifierType type)
		{
			_statsModel.AddPermanentModifier(targetEntId, stat, type, value);
		}

		public void RemoveModifier(StatModId statModId)
		{
			_statsModel.RemoveModifier(statModId);
		}

		public Fix GetModifierValue(StatModId statModId)
		{
			return _statsModel.GetModifierValue(statModId);
		}

		public void RemoveAllModifiersFrom(EntId owner, EntId targetEntId, StatConfig stat)
		{
			_statsModel.RemoveAllModifiersFrom(owner, targetEntId, stat);
		}

		public void ChangeModifier(StatModId statModId, Fix newValue)
		{
			_statsModel.ChangeModifierValue(statModId, newValue);
		}

		public IEnumerable<StatModId> GetModifiersOwnedBy(EntId owner)
		{
			return _statsModel.GetModifiersOwnedBy(owner);
		}

		public IEnumerable<StatModId> GetModifiers(EntId owner, EntId targetEntId)
		{
			return _statsModel.GetModifiersFromOwnerToTarget(owner, targetEntId);
		}

		public Fix SetDepletedValue(EntId targetEntId, StatConfig stat, Fix newValue)
		{
			return _statsModel.SetDepletedValue(targetEntId, stat, newValue);
		}

		public void ResetDepletedValueToMax(EntId targetEntId, StatConfig stat)
		{
			_statsModel.ResetDepletedValueToMax(targetEntId, stat);
		}

		public void ResetDepletedValueToMin(EntId targetEntId, StatConfig stat)
		{
			_statsModel.ResetDepletedValueToMin(targetEntId, stat);
		}

	}
	
	
}
