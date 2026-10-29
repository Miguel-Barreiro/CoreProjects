using System;
using System.Runtime.InteropServices;
using Core.Model;
using Core.Model.ModelSystems;
using Core.Model.Stats;
using Core.Systems;
using Core.VSEngine.Nodes;
using FixedPointy;
using UnityEngine;

#nullable enable

namespace Core.VSEngine.Systems
{
    
    public interface IVSTimersComponent : Component<VSTimersData> { }
    
    [ComponentData(ContainerType = typeof(VSTimersContainer))]
    public struct VSTimersData : IComponentData
    {
        private const int MAX_TIMER_NODES = 2;
        public EntId ID { get; set; }
        public bool Enabled;
     
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=MAX_TIMER_NODES)] 
        public CooldownTimerNode[] CooldownNodes;
        [SerializeField] private uint _CooldownNodesCount;
        public uint CooldownNodesCount
        {
            get => _CooldownNodesCount;
            private set
            {
                if (value > MAX_TIMER_NODES)
                    Debug.LogError($"CooldownNodesCount cannot exceed {MAX_TIMER_NODES}.");
                _CooldownNodesCount = value;
            }
        }
        
        public StatConfig? CooldownStat;

        public CooldownResetType CooldownReset;

        
        public void Init()
        {
            Enabled = true;
            _CooldownNodesCount = 0;
            CooldownNodes = new CooldownTimerNode[MAX_TIMER_NODES];
            CooldownReset = CooldownResetType.Restart;
        }

        public void Reset()
        {
            Enabled = false;
            CooldownReset = CooldownResetType.Restart;
            CooldownNodesCount = 0;
            Array.Clear(CooldownNodes, 0, CooldownNodes.Length);
        }
        
        public void AddCooldownNode(CooldownTimerNode node)
        {
            if (CooldownNodesCount >= MAX_TIMER_NODES)
                throw new InvalidOperationException($"Cannot add more than {MAX_TIMER_NODES} cooldown nodes.");
            CooldownNodes[CooldownNodesCount] = node;
            CooldownNodesCount++;
        }
    }
    
    
    public enum CooldownResetType
    {
        Restart,  // the timer is restarts when it triggers
        Stop	, // the timer is stopped when it triggers until it is started again
    }
    
    public sealed class VSTimersContainer : MultipleComponentEntityContainer<VSTimersData> 
    {
        public VSTimersContainer(uint maxNumber) : base(maxNumber) { }
        
    }
    
    //
    //
    //
    // public readonly struct VSTimerKey : IEquatable<VSTimerKey>
    // {
    //     public readonly EntId Owner;
    //     public readonly CooldownTimerNode Node;
    //
    //     public VSTimerKey(EntId owner, CooldownTimerNode node)
    //     {
    //         Owner = owner; Node = node;
    //     }
    //
    //     public bool Equals(VSTimerKey other) => Owner == other.Owner && ReferenceEquals(Node, other.Node);
    //     public override bool Equals(object obj) => obj is VSTimerKey other && Equals(other);
    //     public override int GetHashCode() => HashCode.Combine(Owner, Node);
    // }

    // public sealed class VSTimer
    // {
    //     public float DurationSeconds;
    //     public float TimeLeftSeconds;
    //
    //     public VSTimer(float durationSeconds)
    //     {
    //         DurationSeconds = durationSeconds;
    //         TimeLeftSeconds = durationSeconds;
    //     }
    // }



}
