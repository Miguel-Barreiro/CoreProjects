using System.Collections.Generic;
using Core.Model;
using Core.Model.ModelSystems;
using FixedPointy;
using UnityEngine;

namespace Core.Systems
{
	public interface TimeScaleSystem
	{
		public void AddTimeScale(Fix percentage, TimeScalerEntity owner);
		public void RemoveTimeScale(TimeScalerEntity owner);
		public Fix GetCurrentTimeScale();
		public void ClearTimeScaleModifiers();
	}

	public struct TimeScalerEntityData : IComponentData
	{
		public EntId ID { get; set; }
	}

	public interface TimeScalerEntity : Component<TimeScalerEntityData> { }


	public sealed class TimeScaleSystemImplementation : OnDestroyComponent<TimeScalerEntityData>,  TimeScaleSystem, IInitSystem
	{
		private TimeScaleSystemModel TimeScaleSystemModel;
		
		public void Initialize()
		{
			if(TimeScaleSystemModel == null)
				TimeScaleSystemModel = new ();
		}


		public void AddTimeScale(Fix percentage, TimeScalerEntity owner)
		{
			if (percentage < Fix.Zero)
			{
				Debug.LogError($"tried to add negative time scale");
				return;
			}
			
			if(TimeScaleSystemModel.scalerByOwners.ContainsKey(owner.ID))
				TimeScaleSystemModel.scalerByOwners[owner.ID] = percentage;
			else
				TimeScaleSystemModel.scalerByOwners.Add(owner.ID, percentage);
			
			UpdateCurrentScale();
		}


		public void OnDestroyComponent(EntId destroyedComponentID)
		{
			if (TimeScaleSystemModel.scalerByOwners.ContainsKey(destroyedComponentID))
			{
				TimeScaleSystemModel.scalerByOwners.Remove(destroyedComponentID);
			}

			UpdateCurrentScale(); 
		}



		public Fix GetCurrentTimeScale()
		{
			return TimeScaleSystemModel.currentScale;
		}

		public void ClearTimeScaleModifiers()
		{
			TimeScaleSystemModel.scalerByOwners.Clear();
			UpdateCurrentScale();
		}

		public void RemoveTimeScale(TimeScalerEntity owner)
		{
			if (TimeScaleSystemModel.scalerByOwners.ContainsKey(owner.ID))
			{
				TimeScaleSystemModel.scalerByOwners.Remove(owner.ID);
				UpdateCurrentScale();
			}
		}

		
		private void UpdateCurrentScale()
		{
			if(TimeScaleSystemModel.scalerByOwners.Count == 0)
			{
				TimeScaleSystemModel.currentScale = Fix.One;
				Time.timeScale = 1;
				return;
			}
				
			Fix minScale = Fix.MaxInteger; 
			foreach ((_, Fix scale) in TimeScaleSystemModel.scalerByOwners)
			{
				if (scale < minScale)
					minScale = scale;
			}

			TimeScaleSystemModel.currentScale = minScale;
			Time.timeScale = (float) TimeScaleSystemModel.currentScale;
		}


		public bool Active { get; set; } = true;
		public SystemGroup Group { get; } = CoreSystemGroups.CoreSystemGroup;
	}

	public sealed class TimeScaleSystemModel : Entity
	{ 
		[SerializeField]
		public Fix currentScale = Fix.One;

		[SerializeField]
		public Dictionary<EntId, Fix> scalerByOwners = new();
		
		
	}

}