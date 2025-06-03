using System;
using System.Collections.Generic;
using Core.Model.ModelSystems;
using Core.Model.Stats;
using Core.Systems;
using Core.Utils;
using Core.Utils.CachedDataStructures;
using FixedPointy;
using UnityEngine;
using Zenject;

#nullable enable

namespace Core.Model.Time
{
	public interface TimerSystem : TimerSystemRo, ISystem
    {
        public void AddOnFinishListener(EntId entityID, string timerId, Action<EntId> onFinishListener);
        public void RemoveOnFinishListener(EntId entityID, string timerId, Action<EntId> onFinishListener);

        public void RemoveTimer(EntId entityID, string timerId);
        
        public void SetTimer(EntId entityID, string id, float expirationMs, bool autoReset, bool isUnscaledTime);
        public void SetTimer(EntId entityID, string id, StatConfig expirationMsStat, bool autoReset, bool isUnscaledTime);
        public void ResetTimer(EntId entityID, string id);

        public void SetGlobalTimerScalerStat(StatConfig timeScalerStat);
    }

	public interface TimerSystemRo
    {
        public bool HasTimer(EntId entId, string id);
        public OperationResult<float> GetMillisecondsLeft(EntId entId, string id);
        public OperationResult<float> GetPercentageLeft(EntId entId, string id);
    }
    

    internal interface ITimerSystemImplementationInternal : ISystem
    {
        public void Update(float deltaTimeMs);
    }
    
    public class TimerSystemImplementation : TimerSystemRo, 
                                             TimerSystem, 
                                             ITimerSystemImplementationInternal, 
                                             IOnDestroyEntitySystem
    {

        [Inject] private readonly StatsSystem StatsSystem = null!;
        [Inject] private readonly TimerModel TimerModel = null!;
        
        
        [NonSerialized]
        private Dictionary<EntId, Dictionary<string, List<Action<EntId>>>> OnFinishListeners = new ();
        
        #region Public

        public void ResetTimer(EntId entityID, string id)
        {
            Dictionary<string, TimerModel.InternalTimer> timersById = GetOrCreateEntityTimers(entityID);
            if (!timersById.TryGetValue(id, out TimerModel.InternalTimer internalTimer))
            {
                return;
            }
            internalTimer.timePassed = 0;
            internalTimer.Running = true;
        }

        public void SetGlobalTimerScalerStat(StatConfig timeScalerStat)
        {
            TimerModel.DefaultTimeScalerStat = timeScalerStat;
        }


        public void SetTimer(EntId entityID, string id, StatConfig expirationMsStat, bool autoReset, bool isUnscaledTime)
        {
            Dictionary<string, TimerModel.InternalTimer> timersById = GetOrCreateEntityTimers(entityID);
            if (!timersById.TryGetValue(id, out TimerModel.InternalTimer internalTimer))
            {
                internalTimer = new TimerModel.InternalTimer(expirationMsStat, id, autoReset, isUnscaledTime);
                timersById[id] = internalTimer;
                return;
            }
            
            internalTimer.CooldownAbsolute = null;
            internalTimer.CooldownStat = expirationMsStat;
            
            internalTimer.isAutoReset = autoReset;
            internalTimer.IsUnscaledTime = isUnscaledTime;
            internalTimer.Running = true;
        }

        public void SetTimer(EntId entityID, string id, float expirationMs, bool autoReset, bool isUnscaledTime)
        {
            Dictionary<string, TimerModel.InternalTimer> timersById = GetOrCreateEntityTimers(entityID);
            if (!timersById.TryGetValue(id, out TimerModel.InternalTimer internalTimer))
            {
                internalTimer = new TimerModel.InternalTimer(expirationMs, id, autoReset, isUnscaledTime);
                timersById[id] = internalTimer;
                return;
            }
            
            internalTimer.CooldownAbsolute = expirationMs;
            internalTimer.CooldownStat = null;
            
            internalTimer.isAutoReset = autoReset;
            internalTimer.IsUnscaledTime = isUnscaledTime;
            internalTimer.Running = true;
        }


        public void RemoveTimer(EntId entityID, string timerId)
        {
            Dictionary<string, TimerModel.InternalTimer> timersById = GetOrCreateEntityTimers(entityID);
            if (timersById.ContainsKey(timerId))
            {
                timersById.Remove(timerId);
            }
        }

        public void AddOnFinishListener(EntId entityID, string timerId, Action<EntId> onFinishListener)
        {
            Dictionary<string, TimerModel.InternalTimer> timersById = GetOrCreateEntityTimers(entityID);
            if (!timersById.TryGetValue(timerId, out TimerModel.InternalTimer internalTimer))
            {
                Debug.Log($"Trying to add a listener to an invalid timer. ([{entityID}].<{timerId})>");
                return;
            }

            List<Action<EntId>> list = GetOrCreateFinishListenersList(entityID, timerId);
            list.Add(onFinishListener);
        }

        private List<Action<EntId>> GetOrCreateFinishListenersList(EntId entityID, string timerId)
        {
            if(!OnFinishListeners.ContainsKey(entityID))
                OnFinishListeners.Add(entityID, new());
            
            if(!OnFinishListeners[entityID].ContainsKey(timerId))
                OnFinishListeners[entityID].Add(timerId, new());
            
            return OnFinishListeners[entityID][timerId];
        }

        public void RemoveOnFinishListener(EntId entityID, string timerId, Action<EntId> onFinishListener)
        {
            if (!OnFinishListeners.ContainsKey(entityID))
                return;

            if (!OnFinishListeners[entityID].ContainsKey(timerId))
                return;

            OnFinishListeners[entityID][timerId].Remove(onFinishListener);
        }


        public bool HasTimer(EntId entId, string id)
        {
            if (!TimerModel.Timers.TryGetValue(entId, out Dictionary<string, TimerModel.InternalTimer> timersById))
            {
                return false;
            }

            return timersById.ContainsKey(id);
        }


        public OperationResult<float> GetMillisecondsLeft(EntId entId, string timerId)
        {
            
            if (!TimerModel.Timers.TryGetValue(entId, out Dictionary<string, TimerModel.InternalTimer> timersById))
                return OperationResult<float>.Failure($"timer not found for entity ([{entId}].<{timerId})>");

            if (!timersById.TryGetValue(timerId, out TimerModel.InternalTimer internalTimer))
                return OperationResult<float>.Failure($"timer not found for entity ([{entId}].<{timerId})>");

            float timeLeftMs;
            if (internalTimer.CooldownStat != null)
            {
                Fix totalCooldown = StatsSystem.GetStatValue(entId, internalTimer.CooldownStat);
                timeLeftMs = Math.Max(0, (float)totalCooldown - internalTimer.timePassed);
            } else
            {
                timeLeftMs = Math.Max(0, internalTimer.CooldownAbsolute!.Value - internalTimer.timePassed);
            }
            
            return OperationResult<float>.Success(timeLeftMs);
        }

        public OperationResult<float> GetPercentageLeft(EntId entId, string timerId)
        {
            if (!TimerModel.Timers.TryGetValue(entId, out Dictionary<string, TimerModel.InternalTimer> timersById))
                return OperationResult<float>.Failure($"timer not found for entity ([{entId}].<{timerId})>");

            if (!timersById.TryGetValue(timerId, out TimerModel.InternalTimer internalTimer))
                return OperationResult<float>.Failure($"timer not found for entity ([{entId}].<{timerId})>");

            float totalCooldown;
            float timeLeftMs;
            if (internalTimer.CooldownStat != null)
            {
                Fix totalCooldownFix = StatsSystem.GetStatValue(entId, internalTimer.CooldownStat);
                totalCooldown = (float)totalCooldownFix;
                timeLeftMs = Math.Max(0, totalCooldown - internalTimer.timePassed);
                
            } else
            {
                totalCooldown = internalTimer.CooldownAbsolute!.Value;
                timeLeftMs = Math.Max(0, internalTimer.CooldownAbsolute!.Value - internalTimer.timePassed);
            }

            if (totalCooldown == 0)
                return OperationResult<float>.Success(0);

            return OperationResult<float>.Success(timeLeftMs/totalCooldown);
        }

        #endregion


        #region Internal

        public void Update(float deltaTime)
        {
            foreach ((EntId entId, Dictionary<string, TimerModel.InternalTimer> internalTimers) in TimerModel.Timers)
            {
                float timerScaleF = 1f;
                if(TimerModel.DefaultTimeScalerStat != null)
                {
                    Fix timerScale = StatsSystem.GetStatValue(entId, TimerModel.DefaultTimeScalerStat);
                    timerScaleF = (float)timerScale;
                }

                foreach ((string id, TimerModel.InternalTimer internalTimer) in internalTimers)
                {
                    if (!internalTimer.Running)
                        continue;

                    if(internalTimer.IsUnscaledTime)
                        internalTimer.timePassed += UnityEngine.Time.unscaledDeltaTime * 1000* timerScaleF;
                    else
                        internalTimer.timePassed += deltaTime * timerScaleF;
                    
                    
                    float totalCooldown;
                    if (internalTimer.CooldownStat != null)
                    {
                        totalCooldown = (float)StatsSystem.GetStatValue(entId, internalTimer.CooldownStat);
                    } else
                    {
                        totalCooldown = internalTimer.CooldownAbsolute!.Value;
                    }
                    
                    if ( internalTimer.timePassed >= totalCooldown)
                    {
                        internalTimer.timePassed -= totalCooldown;

                        if (!internalTimer.isAutoReset)
                        {
                            internalTimer.Running = false;
                            internalTimer.timePassed = totalCooldown;
                        }

                        using CachedList<Action<EntId>> listenersList = ListCache<Action<EntId>>.Get();
                        listenersList.AddRange(GetOrCreateFinishListenersList(entId, internalTimer.Id));

                        foreach (Action<EntId> onFinishListener in listenersList)
                            onFinishListener.Invoke(entId);
                    }
                }
            }
        }
        
        

        public void OnDestroyEntity(EntId destroyedEntityId)
        {
            if (TimerModel.Timers.ContainsKey(destroyedEntityId))
                TimerModel.Timers.Remove(destroyedEntityId);
        }

        
        #endregion


        #region Private


        
        private Dictionary<string, TimerModel.InternalTimer> GetOrCreateEntityTimers(EntId entity)
        {
            if (!TimerModel.Timers.ContainsKey(entity))
            {
                // TODO: cache dictionaries to avoid allocations
                TimerModel.Timers[entity] = new Dictionary<string, TimerModel.InternalTimer>();
            }

            return TimerModel.Timers[entity];
        }

        #endregion


        public bool Active { get; set; } = true;
        public SystemGroup Group { get; } = CoreSystemGroups.CoreSystemGroup;
    }
}