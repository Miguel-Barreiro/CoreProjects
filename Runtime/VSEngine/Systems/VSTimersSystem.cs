using System.Collections.Generic;
using Core.Model;
using Core.Model.ModelSystems;
using Core.Model.Stats;
using Core.Systems;
using Core.Utils.CachedDataStructures;
using Core.VSEngine.Nodes;
using FixedPointy;
using Zenject;

namespace Core.VSEngine.Systems
{
    public interface IVSTimersSystem
    {
        /// <summary> Starts (or restarts) the timer for the given owner/node pair </summary>
        // void StartTimer(EntId owner, StatConfig statConfig);
        // void StartTimer(EntId owner, uint triggerChildOrder);
        
        // void StopTimer(EntId owner, StatConfig statConfig);
        // void StopTimer(EntId owner, uint triggerChildOrder);
        
        // bool IsRunning(EntId owner, StatConfig statConfig);
        // bool IsRunning(EntId owner, uint triggerChildOrder);
        
        Fix GetTimeLeft(EntId owner, StatConfig statConfig);
        Fix GetTimeLeft(EntId owner, uint triggerChildOrder);
        //this is mostly used for unit tests
        void ClearAllTimers();
    }

    public sealed class VSTimersSystem : IVSTimersSystem, UpdateComponents<VSTimersData>
    {
        [Inject] private VSBaseEngine _vsBaseEngine;
        [Inject] private readonly VSTimersContainer VSTimersContainer = null!;
        [Inject] private readonly StatsSystem StatsSystem = null!;

        private StatConfig _defaultTimerStat = null!;
        public void SetDefaultTimerStat(StatConfig statConfig)
        {
            _defaultTimerStat = statConfig;
        }
        
        private struct Timer
        {
            public StatConfig Stat;
            public List<CooldownTimerNode> Nodes;
        } 
        
        
        private readonly Dictionary<StatConfig, Timer> TIMERS_BY_STAT_CACHE = new Dictionary<StatConfig, Timer>();
        
        public void AddTimers(EntId owner, IReadOnlyCollection<ActionGraph> actionGraphs)
        {
            foreach (Timer timer in TIMERS_BY_STAT_CACHE.Values)
                timer.Nodes.Clear();

            if(!TIMERS_BY_STAT_CACHE.TryGetValue(_defaultTimerStat, out Timer mainTimer))
            {
                mainTimer = new Timer
                {
                    Stat = _defaultTimerStat, 
                    Nodes = new List<CooldownTimerNode>()
                };
                TIMERS_BY_STAT_CACHE.Add(_defaultTimerStat, mainTimer);
            }
            bool mainCooldownNeeded = false;
            
            foreach (ActionGraph actionGraph in actionGraphs)
            {
                int triggerIndex = 0;
                foreach (LocalTriggerDefinition localTrigger in actionGraph.LocalTriggers)
                {
                    triggerIndex++;
                    if(triggerIndex == 1)
                        continue;

                    Timer timer;
                    if (!TIMERS_BY_STAT_CACHE.TryGetValue(localTrigger.CooldownStat, out timer))
                    {
                        timer = new Timer();
                        TIMERS_BY_STAT_CACHE[localTrigger.CooldownStat] = timer;
                    }
                }
            }

            foreach (ActionGraph actionGraph in actionGraphs)
            {
                foreach (CooldownTimerNode cooldownTimerNode in actionGraph.GetTimerNodes())
                {
                    StatConfig statConfig = cooldownTimerNode.GetTimerStat() == null? 
                                                _defaultTimerStat : 
                                                cooldownTimerNode.GetTimerStat();
                    Timer timer = TIMERS_BY_STAT_CACHE[statConfig];
                    timer.Nodes.Add(cooldownTimerNode);
                }
            }

            foreach ((StatConfig stat, Timer timer)  in TIMERS_BY_STAT_CACHE)
            {
                if(timer.Nodes.Count == 0)
                    continue;
                
                if (stat == _defaultTimerStat)
                {
                    mainCooldownNeeded = true;
                    continue;
                }

                uint newChildOrder = VSTimersContainer.AddComponent(owner);
                ref VSTimersData childTimerData = ref VSTimersContainer.GetComponentByChildOrder(owner, newChildOrder);
                childTimerData.CooldownStat = stat;
                for (int i = 0; i < timer.Nodes.Count; i++)
                    childTimerData.AddCooldownNode(timer.Nodes[i]);
            }
            
            uint mainCooldownIndex = VSTimersContainer.GetChildIndex(owner, 0);
            ref VSTimersData mainCooldownData = ref VSTimersContainer.Components[mainCooldownIndex];
            mainCooldownData.Enabled = mainCooldownNeeded;
        }
        
        public void UpdateComponents(float deltaTime)
        {

            uint topIndex = VSTimersContainer.TopEmptyIndex;
            for (int i = 0; i < topIndex; i++)
            {
                ref VSTimersData timerData = ref VSTimersContainer.Components[i];
                EntId timerDataID = timerData.ID;
                
                if(!timerData.Enabled)
                    continue;

                Fix newCooldown = StatsSystem.ChangeDepletedValue(timerDataID, timerData.CooldownStat, -deltaTime);
                if (newCooldown < 0)
                {
                    switch (timerData.CooldownReset)
                    {
                        case CooldownResetType.Restart:
                            Fix totalCooldown = StatsSystem.GetStatValue(timerDataID, timerData.CooldownStat); 
                            StatsSystem.ChangeDepletedValue(timerDataID, timerData.CooldownStat, totalCooldown);
                            break;
                        case CooldownResetType.Stop:
                            StatsSystem.ResetDepletedValueToMax(timerDataID, timerData.CooldownStat);
                            timerData.Enabled = false;
                            break;
                    }
                    foreach (CooldownTimerNode timerNode in timerData.CooldownNodes)
                    {
                        _vsBaseEngine.RunFromNode(timerNode, timerDataID);
                    }
                }
            }
        }
        
        
        public bool Active { get; set; } = true;
        public SystemGroup Group { get; } = CoreSystemGroups.CoreSystemGroup;



        // public void StartTimer(EntId owner, uint triggerChildOrder)
        // {
        //     ref VSTimersData timersData = ref VSTimersContainer.GetComponentByChildOrder(owner, triggerChildOrder);
        //     timersData.Enabled = true;
        //     StatsSystem.ResetDepletedValueToMax(owner, timersData.CooldownStat);
        // }
        //
        // public void StopTimer(EntId owner, uint triggerChildOrder)
        // {
        //     ref VSTimersData timersData = ref VSTimersContainer.GetComponentByChildOrder(owner, triggerChildOrder);
        //     timersData.Enabled = false;
        // }
        //
        // public bool IsRunning(EntId owner, uint triggerChildOrder)
        // {
        //     ref VSTimersData timersData = ref VSTimersContainer.GetComponentByChildOrder(owner, triggerChildOrder);
        //     return timersData.Enabled;
        // }

        public Fix GetTimeLeft(EntId owner, StatConfig cooldownStat)
        {
            return StatsSystem.GetStatDepletedValue(owner, cooldownStat);
        }

        public Fix GetTimeLeft(EntId owner, uint triggerChildOrder)
        {
            ref VSTimersData timersData = ref VSTimersContainer.GetComponentByChildOrder(owner, triggerChildOrder);
            return StatsSystem.GetStatDepletedValue(owner, timersData.CooldownStat);
        }
        public void ClearAllTimers()
        {
            VSTimersContainer.Reset();

        }
    }
}
