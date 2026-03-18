using System;
using System.Collections.Generic;
using Core.Model;
using Core.Model.ModelSystems;
using Core.VSEngine;

namespace Core.VSEngine.Systems
{
    // ListenerMap: eventType → ownerId → nodes
    using ListenerMap = Dictionary<Type, Dictionary<EntId, List<BaseEventListenNode>>>;

    public struct VSEventListenersData : IComponentData
    {
        public EntId ID { get; set; }

        // Global event maps (BaseEvent) — one per timing, keyed by owner
        public ListenerMap preGlobal;
        public ListenerMap defaultGlobal;
        public ListenerMap postGlobal;

        // Entity event maps:
        //   Owner:   keyed by target entity (== owner), O(1) lookup via ev.EntityID
        //   All:     keyed by owner, iterated entirely on trigger
        //   Dynamic: keyed by owner, iterated + checked against GetDynamicTargetEntity() at trigger time
        public ListenerMap preOwner, defaultOwner, postOwner;
        public ListenerMap preAll, defaultAll, postAll;
        public ListenerMap preDynamic, defaultDynamic, postDynamic;

        // Owner index for fast bulk removal when an entity is destroyed
        public Dictionary<EntId, List<OwnerEntry>> ownerIndex;

        public void Init()
        {
            preGlobal     = new();
            defaultGlobal = new();
            postGlobal    = new();

            preOwner    = new(); defaultOwner    = new(); postOwner    = new();
            preAll      = new(); defaultAll      = new(); postAll      = new();
            preDynamic  = new(); defaultDynamic  = new(); postDynamic  = new();

            ownerIndex = new();
        }

        public readonly struct OwnerEntry
        {
            public readonly ListenerMap Map;
            public readonly Type EventType;
            public readonly BaseEventListenNode Node;
            public OwnerEntry(ListenerMap map, Type eventType, BaseEventListenNode node)
            {
                Map = map; EventType = eventType; Node = node;
            }
        }
    }

    public interface IVSEventListenersComponent : Component<VSEventListenersData> { }

    public sealed class VSEventListenersEntity : Entity, IVSEventListenersComponent
    {
        public ref VSEventListenersData GetData() => ref GetComponent<VSEventListenersData>();
    }
}
