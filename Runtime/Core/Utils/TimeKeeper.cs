using System;
using System.Collections.Generic;
using Core.Utils.CachedDataStructures;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Utils
{
    public sealed class TimeKeeper : MonoBehaviour, ITimeKeeper
    {
        private double currentTimePassed;
        private Dictionary<string, InternalTimer> currentTimers = new Dictionary<string, InternalTimer>();

        private sealed class InternalTimer
        {
            internal double TimeLeftMs;
            internal UniTaskCompletionSource<bool> TimerResultTask;
            internal string Tag;
            internal Action OnFinishCallback { get; set; }

            public InternalTimer(double timeLeftMs, string tag)
            {
                TimeLeftMs = timeLeftMs;
                TimerResultTask = new();
                Tag = tag;
            }
        }
        
        public void Update()
        {
            double deltaTime = Time.unscaledDeltaTime*1000;
            currentTimePassed += deltaTime;
            
            using CachedList<string> endedTimerNames = ListCache<string>.Get();
            using CachedList<InternalTimer> endedNamelessTimers = ListCache<InternalTimer>.Get();

            foreach ((string timerName, InternalTimer internalTimer) in currentTimers)
            {
                if (internalTimer.TimeLeftMs > 0)
                {
                    internalTimer.TimeLeftMs -= deltaTime;
                    if (internalTimer.TimeLeftMs < 0)
                    {
                        endedTimerNames.Add(timerName);
                    }
                }
            }

            foreach (string timerName in endedTimerNames)
            {
                InternalTimer internalTimer = currentTimers[timerName];
                internalTimer.OnFinishCallback?.Invoke();
                internalTimer.OnFinishCallback = null;
                currentTimers.Remove(timerName);
                internalTimer.TimerResultTask.TrySetResult(true);
            }
        }

        public void AddOnFinishListener(string timerId, Action onFinishListener)
        {
            if (!HasTimer(timerId))
            {
                Debug.Log($"Trying to add a listener to an invalid timer. Timer id: ${timerId}");
                return;
            }

            InternalTimer timer = currentTimers[timerId];
            timer.OnFinishCallback += onFinishListener;
        }

        public void RemoveOnFinishListener(string timerId, Action onFinishListener)
        {
            if (!HasTimer(timerId))
            {
                Debug.Log($"Trying to remove a listener from an invalid timer. Timer id: ${timerId}");
                return;
            }

            InternalTimer timer = currentTimers[timerId];
            try
            {
                timer.OnFinishCallback -= onFinishListener;
            }
            catch (Exception)
            {
                Debug.Log($"Trying to remove a listener from an invalid timer. Timer id: ${timerId}");
            }
        }

        public void SetTimer(string id, double expirationMs, string tag ="")
        {
            if (!currentTimers.ContainsKey(id))
            {
                currentTimers[id] = new InternalTimer(expirationMs, tag);
            }
            else
            {
                InternalTimer internalTimer = currentTimers[id];
                if (internalTimer.TimerResultTask != null)
                {
                    if (internalTimer.TimerResultTask.Task.Status != UniTaskStatus.Pending)
                    {
                        internalTimer.TimerResultTask = new UniTaskCompletionSource<bool>();
                    }
                }
                internalTimer.TimeLeftMs = expirationMs;
            }
        }

        public UniTask<bool> GetWaitTimerResult(string id)
        {
            if (!currentTimers.ContainsKey(id))
            {
                currentTimers[id] = new InternalTimer(-1,"");
            }

            return currentTimers[id].TimerResultTask.Task;
        }
        
        public bool HasTimer(string id)
        {
            return currentTimers.ContainsKey(id);
        }

        public double GetMillisecondsLeft(string id)
        {
            if (!currentTimers.ContainsKey(id))
            {
                currentTimers[id] = new InternalTimer(-1,"");
            }

            return currentTimers[id].TimeLeftMs;
        }

        public void RemoveTimersByTag(string tag)
        {
            List<string> keysToRemove = new List<string>();
            foreach ((string key, InternalTimer value) in currentTimers)
            {
                if (value.Tag == tag)
                {
                    keysToRemove.Add(key);
                }
            }

            for (int i = 0; i < keysToRemove.Count; i++)
            {
                RemoveTimer(keysToRemove[i]);
            }
            
        }

        public void RemoveTimer(string id)
        {
            if (currentTimers.ContainsKey(id))
            {
                currentTimers[id].TimerResultTask.TrySetResult(false);
                currentTimers.Remove(id);
            }
        }
    }
    
    public interface ITimeKeeper
    {
        public void SetTimer(string id, double expiration, string tag);
        // If the result false the timer is not trigger normally it was removed before the timer run out.
        public UniTask<bool> GetWaitTimerResult(string id);
        public double GetMillisecondsLeft(string id);
    }
}