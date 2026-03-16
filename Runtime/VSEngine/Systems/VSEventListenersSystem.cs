using System;
using System.Collections.Generic;
using Core.Events;
using Core.Model;
using Core.Model.ModelSystems;
using Core.Utils;
using Core.VSEngine;
using UnityEngine;
using Zenject;

namespace Core.VSEngine.Systems
{
    // ListenerMap: eventType → ownerId → nodes
    using ListenerMap = Dictionary<Type, Dictionary<EntId, List<BaseEventListenNode>>>;

    public interface IVSEventListenersSystem
    {
        void AddListener(EntId owner, Type eventType, BaseEventListenNode node);
        void RemoveListener(EntId owner, Type eventType, BaseEventListenNode node);
    }

    public sealed class VSEventListenersSystem : IVSEventListenersSystem, IOnDestroyEntitySystem
    {
        [Inject] private VSBaseEngine _vsBaseEngine;

        // Global event maps (BaseEvent) — one per timing, keyed by owner
        private readonly ListenerMap preGlobal     = new();
        private readonly ListenerMap defaultGlobal = new();
        private readonly ListenerMap postGlobal    = new();

        // Entity event maps:
        //   Owner:   keyed by target entity (== owner), O(1) lookup via ev.EntityID
        //   All:     keyed by owner, iterated entirely on trigger
        //   Dynamic: keyed by owner, iterated + checked against GetDynamicTargetEntity() at trigger time
        private readonly ListenerMap preOwner    = new(), defaultOwner    = new(), postOwner    = new();
        private readonly ListenerMap preAll      = new(), defaultAll      = new(), postAll      = new();
        private readonly ListenerMap preDynamic  = new(), defaultDynamic  = new(), postDynamic  = new();

        // Owner index for fast bulk removal when an entity is destroyed
        private readonly Dictionary<EntId, List<OwnerEntry>> ownerIndex = new();

        private readonly struct OwnerEntry
        {
            public readonly ListenerMap Map;
            public readonly Type EventType;
            public readonly BaseEventListenNode Node;
            public OwnerEntry(ListenerMap map, Type eventType, BaseEventListenNode node)
            {
                Map = map; EventType = eventType; Node = node;
            }
        }

        // --- Timing → map helpers ---
        private ListenerMap GlobalMap(VSEventTiming t) => t switch
        {
            VSEventTiming.Pre  => preGlobal,
            VSEventTiming.Post => postGlobal,
            _                  => defaultGlobal,
        };

        private ListenerMap OwnerMap(VSEventTiming t) => t switch
        {
            VSEventTiming.Pre  => preOwner,
            VSEventTiming.Post => postOwner,
            _                  => defaultOwner,
        };

        private ListenerMap AllMap(VSEventTiming t) => t switch
        {
            VSEventTiming.Pre  => preAll,
            VSEventTiming.Post => postAll,
            _                  => defaultAll,
        };

        private ListenerMap DynamicMap(VSEventTiming t) => t switch
        {
            VSEventTiming.Pre  => preDynamic,
            VSEventTiming.Post => postDynamic,
            _                  => defaultDynamic,
        };

        // --- Global event hooks (called by SystemsController) ---
        public void ExecutePreEvent(BaseEvent ev)  => TriggerGlobal(ev, preGlobal);
        public void ExecuteEvent(BaseEvent ev)     => TriggerGlobal(ev, defaultGlobal);
        public void ExecutePostEvent(BaseEvent ev) => TriggerGlobal(ev, postGlobal);

        private void TriggerGlobal(BaseEvent ev, ListenerMap map)
        {
            if (!map.TryGetValue(ev.GetType(), out var byOwner)) return;
            foreach (var (ownerId, nodes) in byOwner)
                foreach (var node in nodes)
                    if (node.CanExecute(null!, ownerId))
                        _vsBaseEngine.RunEvent(node, ev, ownerId);
        }

        // --- Entity event hooks (called by EntityEventQueueImplementation<T>) ---
        public void ExecutePreEntityEvent<T>(T ev)    where T : EntityEvent<T>, new()
            => TriggerEntity(ev, preOwner, preAll, preDynamic);
        public void ExecuteEntityEvent<T>(T ev)       where T : EntityEvent<T>, new()
            => TriggerEntity(ev, defaultOwner, defaultAll, defaultDynamic);
        public void ExecutePostEntityEvent<T>(T ev)   where T : EntityEvent<T>, new()
            => TriggerEntity(ev, postOwner, postAll, postDynamic);

        private void TriggerEntity<T>(T ev, ListenerMap ownerMap, ListenerMap allMap, ListenerMap dynamicMap)
            where T : EntityEvent<T>, new()
        {
            Type t = typeof(T);

            // Owner: O(1) lookup — only fire nodes registered for this exact entity
            if (ownerMap.TryGetValue(t, out var byTarget) &&
                byTarget.TryGetValue(ev.EntityID, out var ownerNodes))
                foreach (var node in ownerNodes)
                    if (node.CanExecute(null!, ev.EntityID))
                        _vsBaseEngine.RunEntityEvent(node, ev, ev.EntityID);

            // All: fire for every registered owner
            if (allMap.TryGetValue(t, out var allByOwner))
                foreach (var (ownerId, nodes) in allByOwner)
                    foreach (var node in nodes)
                        if (node.CanExecute(null!, ownerId))
                            _vsBaseEngine.RunEntityEvent(node, ev, ownerId);

            // Dynamic: fire if ev.EntityID matches the node's runtime input port value
            if (dynamicMap.TryGetValue(t, out var dynByOwner))
                foreach (var (ownerId, nodes) in dynByOwner)
                    foreach (var node in nodes)
                    {
                        if (node is EntityEventListenNode entityListenNode &&
                            node.CanExecute(null!, ownerId))
                        {
                            OperationResult<EntId> dynamicTargetEntity = entityListenNode.GetDynamicTargetEntity();
                            if(dynamicTargetEntity.IsFailure)
                            {
                                Debug.LogError($"Failed to resolve dynamic target entity for node {node.name}: {dynamicTargetEntity.Exception.Message}");
                                continue;
                            }
                            if (dynamicTargetEntity.Result == ev.EntityID)
                                _vsBaseEngine.RunEntityEvent(node, ev, ownerId);
                        }

                    }
        }

        // --- Listener registration ---
        public void AddListener(EntId owner, Type eventType, BaseEventListenNode node)
        {
            ListenerMap map = SelectMap(node);
            Insert(map, eventType, owner, node);

            if (!ownerIndex.TryGetValue(owner, out var entries))
                ownerIndex[owner] = entries = new List<OwnerEntry>();
            entries.Add(new OwnerEntry(map, eventType, node));
        }

        public void RemoveListener(EntId owner, Type eventType, BaseEventListenNode node)
        {
            ListenerMap map = SelectMap(node);
            if (map.TryGetValue(eventType, out var byKey) &&
                byKey.TryGetValue(owner, out var nodes))
                nodes.Remove(node);
        }

        // Fast bulk removal when an entity is destroyed
        public void OnDestroyEntity(EntId destroyedId)
        {
            if (!ownerIndex.TryGetValue(destroyedId, out var entries)) return;
            foreach (var entry in entries)
                if (entry.Map.TryGetValue(entry.EventType, out var byKey) &&
                    byKey.TryGetValue(destroyedId, out var nodes))
                    nodes.Remove(entry.Node);
            ownerIndex.Remove(destroyedId);
        }

        private ListenerMap SelectMap(BaseEventListenNode node)
        {
            if (node is EntityEventListenNode eeln)
                return eeln.ListenTarget switch
                {
                    VSEntityListenTarget.All     => AllMap(eeln.Timing),
                    VSEntityListenTarget.Dynamic => DynamicMap(eeln.Timing),
                    _                            => OwnerMap(eeln.Timing),   // Owner + Parent (future)
                };
            // Global BaseEvent node
            return GlobalMap(node is EventListenNode eln ? eln.Timing : VSEventTiming.Default);
        }

        private static void Insert(ListenerMap map, Type eventType, EntId key, BaseEventListenNode node)
        {
            if (!map.TryGetValue(eventType, out var byKey))
                map[eventType] = byKey = new Dictionary<EntId, List<BaseEventListenNode>>();
            if (!byKey.TryGetValue(key, out var nodes))
                byKey[key] = nodes = new List<BaseEventListenNode>();
            nodes.Add(node);
        }
    }
}
