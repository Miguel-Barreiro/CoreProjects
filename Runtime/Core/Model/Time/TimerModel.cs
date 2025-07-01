using System;
using System.Collections.Generic;
using Core.Model.Stats;
using UnityEngine;

namespace Core.Model.Time
{
	public class TimerModel : Entity
	{
		[SerializeField]
		internal Dictionary<EntId, Dictionary<string, InternalTimer>> Timers = new ();
		
		internal StatConfig DefaultTimeScalerStat = null;
		
		[Serializable]
		internal sealed class InternalTimer
		{
			internal EntId EntityID;
			internal string Id;

			internal float? CooldownAbsolute = null;
			[SerializeField]
			internal StatConfig? CooldownStat = null;
            
			internal float timePassed = 0;
            
			public bool Running = true;

			internal bool isAutoReset = false;
			internal bool IsUnscaledTime = true;
			
			public InternalTimer(float timeMs, EntId entityID,  string id, bool autoReset, bool isUnscaledTime)
			{
				Id = id;
				CooldownStat = null;
				CooldownAbsolute = timeMs;
				IsUnscaledTime = isUnscaledTime;
				isAutoReset = autoReset;
				EntityID = entityID;
			}
            
			public InternalTimer(StatConfig cooldownStat, EntId entityID, string id, bool autoReset, bool isUnscaledTime)
			{
				Id = id;
				CooldownAbsolute = null;
				CooldownStat = cooldownStat;
				IsUnscaledTime = isUnscaledTime;
				isAutoReset = autoReset;
				EntityID = entityID;
			}

		}
	}
}