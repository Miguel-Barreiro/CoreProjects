#nullable enable

using System;
using System.Collections.Generic;
using Core.Events;
using Core.Model;
using Core.Utils;
using Core.VSEngine.Nodes;
using Core.VSEngine.Nodes.Events;
using UnityEngine;
using XNode;

namespace Core.VSEngine
{
    [Node.NodeWidth(200)]
    [Node.CreateNodeMenu(VSNodeMenuNames.EVENTS_MENU+"/[Listen] Event", order = 2)]
    [Node.NodeTint(VSNodeMenuNames.LISTEN_NODES_TINT)]
    [Serializable]
    public class EventListenNode : BaseEventListenNode, IValueNode
    {
        [SerializeField] private EventSerializedType selectedEventType;
        [SerializeField, HideInInspector] private List<VSFieldPort> vsFieldPorts = new();
        [SerializeField] public VSEventTiming Timing = VSEventTiming.Default;

        private Dictionary<string, VSFieldPort>? fieldCache;

        public Type? EventType => SerializedTypeUtils.GetParsedType(selectedEventType);

        public OperationResult<object> GetValue(string portName)
        {
            IBaseEvent? ev = GetCoreEvent();
            if (ev == null)
                return OperationResult<object>.Failure($"No BaseEvent found in {name}");

            if (fieldCache == null)
            {
                fieldCache = new();
                EventNodeUtils.BuildFieldCache(fieldCache, vsFieldPorts);
            }

            return EventNodeUtils.ReadFromObject(portName, fieldCache, ev);
        }

        // public override void Register(Type _, EntId ownerId)
        //     => base.Register(EventType!, ownerId);
        //
        // public override void DeRegister(Type _, EntId ownerId)
        //     => base.DeRegister(EventType!, ownerId);

#if UNITY_EDITOR
        private void BuildDynamicPorts()
        {
            Type? t = EventType;
            if (t == null) return;
            EventNodeUtils.CreateFieldPorts(t, vsFieldPorts, false);
            EventNodeUtils.AddDynamicPorts(this, vsFieldPorts);
        }

        public override void OnBeforeSerialize()
        {
            BuildDynamicPorts();
            base.OnBeforeSerialize();
        }
#endif
    }
}
