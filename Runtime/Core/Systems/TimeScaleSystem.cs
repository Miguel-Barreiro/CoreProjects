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

	public interface TimeScalerEntity : IComponent { }


	public sealed class TimeScaleSystemImplementation : ComponentSystem<TimeScalerEntity>,  TimeScaleSystem, IInitSystem
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

		public override void OnNew(TimeScalerEntity newComponent) {}

		public override void OnDestroy(TimeScalerEntity newComponent)
		{
			if (TimeScaleSystemModel.scalerByOwners.ContainsKey(newComponent.ID))
			{
				TimeScaleSystemModel.scalerByOwners.Remove(newComponent.ID);
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

		public override void Update(TimeScalerEntity component, float deltaTime) 
		{
			// No per-frame update needed for this system
		}

		
		
		
		
		
		
		public override SystemGroup Group { get; } = CoreSystemGroups.CoreViewEntitySystemGroup;
	
		
		
		
		
		private void UpdateCurrentScale()
		{
			if(TimeScaleSystemModel.scalerByOwners.Count == 0)
			{
				TimeScaleSystemModel.currentScale = Fix.One;
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
	}

	public sealed class TimeScaleSystemModel : BaseEntity
	{ 
		[SerializeField]
		public Fix currentScale = Fix.One;

		[SerializeField]
		public Dictionary<EntId, Fix> scalerByOwners = new();
		
		
	}

}