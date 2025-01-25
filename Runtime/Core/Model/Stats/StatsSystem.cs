using Core.Systems;
using FixedPointy;

namespace Core.Model
{

	public interface StatsSystem
	{

		public void SetBaseValue(EntId targetEntId, StatConfig stat, Fix newValue);
		public void ChangeDepletedValue(EntId targetEntId, StatConfig stat, Fix delta);
		
		
		public Fix GetStatValue(EntId targetEntId, StatConfig stat);
		public Fix GetStatDepletedValue(EntId targetEntId, StatConfig stat);
		
		
		public StatModId AddModifier(EntId owner, EntId targetEntId, StatConfig stat, Fix value, StatModifierType type);
		public void RemoveModifier(StatModId statModId);
		
	}
	


	public class StatsSystemImplementation : EntitySystem<BaseEntity>, StatsSystem
	{
		private readonly StatsModel _statsModel;
	
		public StatsSystemImplementation()
		{ 
			_statsModel = new StatsModel();
		}

		public override SystemGroup Group => CoreSystemGroups.CoreSystemGroup;

		public override void OnNew(BaseEntity newEntity)
		{
			
		}

		public override void OnDestroy(BaseEntity entity)
		{
			_statsModel.RemoveAllModifiersFrom(entity.ID); 
			_statsModel.RemoveAllStatsFrom(entity.ID);
		}

		public override void Update(BaseEntity entity, float deltaTime)
		{
			// Time-based stat updates if needed
		}

		public void SetBaseValue(EntId targetEntId, StatConfig stat, Fix newValue)
		{
			_statsModel.SetBaseValue(targetEntId, stat, newValue);
		}

		public void ChangeDepletedValue(EntId targetEntId, StatConfig stat, Fix delta)
		{
			_statsModel.ModifyDepletedValue(targetEntId, stat, delta);
		}

		public Fix GetStatValue(EntId targetEntId, StatConfig stat)
		{
			return _statsModel.GetStatValue(targetEntId, stat);
		}

		public Fix GetStatDepletedValue(EntId targetEntId, StatConfig stat)
		{
			return _statsModel.GetDepletedStatValue(targetEntId, stat);
		}

		public StatModId AddModifier(EntId owner, EntId targetEntId, StatConfig stat, Fix modifierValue, StatModifierType type)
		{
			return _statsModel.AddModifier(owner, targetEntId, stat, type, modifierValue);
		}
		
		public void RemoveModifier(StatModId statModId)
		{
			_statsModel.RemoveModifier(statModId);
		}
	}
	
	
}
