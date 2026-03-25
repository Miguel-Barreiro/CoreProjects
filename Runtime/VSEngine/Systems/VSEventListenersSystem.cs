using System;
using System.Collections.Generic;
using Core.Events;
using Core.Model;
using Core.Model.ModelSystems;
using Core.Utils;
using Core.VSEngine;
using Core.VSEngine.Nodes.Events;
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
        
        //this is mostly used for unit tests
        void ClearAllListeners();
    }

    public sealed class VSEventListenersSystem : IVSEventListenersSystem, IOnDestroyEntitySystem
    {
        [Inject] private VSBaseEngine _vsBaseEngine;
        [Inject] private VSEventListenersEntity _entity;

        private ref VSEventListenersData Data => ref _entity.GetData();

        // --- Timing → map helpers ---
        private ListenerMap GlobalMap(VSEventTiming t) => t switch
        {
            VSEventTiming.Pre  => Data.preGlobal,
            VSEventTiming.Post => Data.postGlobal,
            _                  => Data.defaultGlobal,
        };

        private ListenerMap OwnerMap(VSEventTiming t) => t switch
        {
            VSEventTiming.Pre  => Data.preOwner,
            VSEventTiming.Post => Data.postOwner,
            _                  => Data.defaultOwner,
        };

        private ListenerMap AllMap(VSEventTiming t) => t switch
        {
            VSEventTiming.Pre  => Data.preAll,
            VSEventTiming.Post => Data.postAll,
            _                  => Data.defaultAll,
        };

        private ListenerMap DynamicMap(VSEventTiming t) => t switch
        {
            VSEventTiming.Pre  => Data.preDynamic,
            VSEventTiming.Post => Data.postDynamic,
            _                  => Data.defaultDynamic,
        };

        // --- Global event hooks (called by SystemsController) ---
        public void ExecutePreEvent(BaseEvent ev)  => TriggerGlobal(ev, Data.preGlobal);
        public void ExecuteEvent(BaseEvent ev)     => TriggerGlobal(ev, Data.defaultGlobal);
        public void ExecutePostEvent(BaseEvent ev) => TriggerGlobal(ev, Data.postGlobal);

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
            => TriggerEntity(ev, Data.preOwner, Data.preAll, Data.preDynamic);
        public void ExecuteEntityEvent<T>(T ev)       where T : EntityEvent<T>, new()
            => TriggerEntity(ev, Data.defaultOwner, Data.defaultAll, Data.defaultDynamic);
        public void ExecutePostEntityEvent<T>(T ev)   where T : EntityEvent<T>, new()
            => TriggerEntity(ev, Data.postOwner, Data.postAll, Data.postDynamic);

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

            if (!Data.ownerIndex.TryGetValue(owner, out var entries))
                Data.ownerIndex[owner] = entries = new List<VSEventListenersData.OwnerEntry>();
            entries.Add(new VSEventListenersData.OwnerEntry(map, eventType, node));
        }

        public void RemoveListener(EntId owner, Type eventType, BaseEventListenNode node)
        {
            ListenerMap map = SelectMap(node);
            if (map.TryGetValue(eventType, out var byKey) &&
                byKey.TryGetValue(owner, out var nodes))
                nodes.Remove(node);
        }

        public void ClearAllListeners()
        {
            Data.preGlobal.Clear();
            Data.defaultGlobal.Clear();
            Data.postGlobal.Clear();
            Data.preOwner.Clear();
            Data.defaultOwner.Clear();
            Data.postOwner.Clear();
            Data.preAll.Clear();
            Data.defaultAll.Clear();
            Data.postAll.Clear();
            Data.preDynamic.Clear();
            Data.defaultDynamic.Clear();
            Data.postDynamic.Clear();
            Data.ownerIndex.Clear();
        }

        // Fast bulk removal when an entity is destroyed
        public void OnDestroyEntity(EntId destroyedId)
        {
            if (!Data.ownerIndex.TryGetValue(destroyedId, out var entries)) return;
            foreach (var entry in entries)
                if (entry.Map.TryGetValue(entry.EventType, out var byKey) &&
                    byKey.TryGetValue(destroyedId, out var nodes))
                    nodes.Remove(entry.Node);
            Data.ownerIndex.Remove(destroyedId);
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
