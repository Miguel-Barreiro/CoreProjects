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
	public interface TimerSystem : TimerSystemRo
    {
        public void AddOnFinishListener(ITimerComponent entity, string timerId, Action<EntId> onFinishListener);
        public void RemoveOnFinishListener(ITimerComponent entity, string timerId, Action<EntId> onFinishListener);

        public void RemoveTimer(ITimerComponent entity, string timerId);
        
        public void SetTimer(ITimerComponent entity, string id, float expirationMs, bool autoReset, bool isUnscaledTime);
        public void SetTimer(ITimerComponent entity, string id, StatConfig expirationMsStat, bool autoReset, bool isUnscaledTime);
        public void ResetTimer(ITimerComponent entity, string id);
    }

	public interface TimerSystemRo
    {
        public bool HasTimer(EntId entId, string id);
        public OperationResult<float> GetMillisecondsLeft(EntId entId, string id);
    }

    internal interface ITimerSystemImplementationInternal
    {
        public void Update(float deltaTimeMs);
    }

    public interface ITimerComponent : IComponent { }

    public class TimerSystemImplementation : ComponentSystem<ITimerComponent>,
                                             TimerSystemRo, 
                                             TimerSystem, 
                                             ITimerSystemImplementationInternal
	{

        [Inject] private readonly StatsSystem StatsSystem = null!;
        [Inject] private readonly TimerModel TimerModel = null!;
        
        
        [NonSerialized]
        private Dictionary<EntId, Dictionary<string, List<Action<EntId>>>> OnFinishListeners = new ();
        
        
        
        #region Public

        public void ResetTimer(ITimerComponent entity, string id)
        {
            Dictionary<string, TimerModel.InternalTimer> timersById = GetOrCreateEntityTimers(entity.ID);
            if (!timersById.TryGetValue(id, out TimerModel.InternalTimer internalTimer))
            {
                return;
            }
            internalTimer.timePassed = 0;
            internalTimer.Running = true;
        }

        public void SetTimer(ITimerComponent entity, string id, StatConfig expirationMsStat, bool autoReset, bool isUnscaledTime)
        {
            Dictionary<string, TimerModel.InternalTimer> timersById = GetOrCreateEntityTimers(entity.ID);
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

        public void SetTimer(ITimerComponent entity, string id, float expirationMs, bool autoReset, bool isUnscaledTime)
        {
            Dictionary<string, TimerModel.InternalTimer> timersById = GetOrCreateEntityTimers(entity.ID);
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

        public void RemoveTimer(ITimerComponent entity, string timerId)
        {
            Dictionary<string, TimerModel.InternalTimer> timersById = GetOrCreateEntityTimers(entity.ID);
            if (timersById.ContainsKey(timerId))
            {
                timersById.Remove(timerId);
            }
        }

        public void AddOnFinishListener(ITimerComponent entity, string timerId, Action<EntId> onFinishListener)
        {
            Dictionary<string, TimerModel.InternalTimer> timersById = GetOrCreateEntityTimers(entity.ID);
            if (!timersById.TryGetValue(timerId, out TimerModel.InternalTimer internalTimer))
            {
                Debug.Log($"Trying to add a listener to an invalid timer. ([{entity.ID}].<{timerId})>");
                return;
            }

            List<Action<EntId>> list = GetOrCreateFinishListenersList(entity.ID, timerId);
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

        public void RemoveOnFinishListener(ITimerComponent entity, string timerId, Action<EntId> onFinishListener)
        {
            if (!OnFinishListeners.ContainsKey(entity.ID))
                return;

            if (!OnFinishListeners[entity.ID].ContainsKey(timerId))
                return;

            OnFinishListeners[entity.ID][timerId].Remove(onFinishListener);
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
        
        
        
        

        #endregion


        #region Internal

        public void Update(float deltaTime)
        {
            foreach ((EntId entId, Dictionary<string, TimerModel.InternalTimer> internalTimers) in TimerModel.Timers)
            {
                foreach ((string id, TimerModel.InternalTimer internalTimer) in internalTimers)
                {
                    if (!internalTimer.Running)
                        continue;

                    if(internalTimer.IsUnscaledTime)
                        internalTimer.timePassed += UnityEngine.Time.unscaledDeltaTime * 1000;
                    else
                        internalTimer.timePassed += deltaTime;
                    
                    
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
        

        public override void OnNew(ITimerComponent newComponent) { }

        public override void OnDestroy(ITimerComponent newComponent)
        {
            if (TimerModel.Timers.ContainsKey(newComponent.ID))
                TimerModel.Timers.Remove(newComponent.ID);
        }

        public override void Update(ITimerComponent component, float deltaTime) { }
        
        #endregion


        #region Private


        
        private Dictionary<string, TimerModel.InternalTimer> GetOrCreateEntityTimers(EntId entity)
        {
            if (!TimerModel.Timers.ContainsKey(entity))
            {
                TimerModel.Timers[entity] = new Dictionary<string, TimerModel.InternalTimer>();
            }

            return TimerModel.Timers[entity];
        }

        #endregion
        

        public override SystemGroup Group { get; } = CoreSystemGroups.CoreSystemGroup;
    }
}