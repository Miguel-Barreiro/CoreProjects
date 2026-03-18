#nullable enable

using System;
using System.Collections.Generic;
using Core.Events;
using Core.Model;
using Core.Utils;
using UnityEngine;
using XNode;

namespace Core.VSEngine
{
    [Node.NodeTint("#215C32")]
    [Node.NodeWidth(300)]
    [Node.CreateNodeMenu("Core/events/EventEntityListenNode")]
    [Serializable]
    public class EntityEventListenNode : BaseEventListenNode, IValueNode
    {
        private const string TARGET_ENTITY_INPUT_NAME = "Target";

        [SerializeField] private SerializedType selectedEventType;
        [SerializeField, HideInInspector] private List<VSFieldPort> vsFieldPorts = new();
        [SerializeField] public VSEventTiming Timing = VSEventTiming.Default;
        [SerializeField] public VSEntityListenTarget ListenTarget = VSEntityListenTarget.Owner;

        private Dictionary<string, VSFieldPort>? fieldCache;

        public Type? EventType => SerializedTypeUtils.GetParsedType(selectedEventType);

        // For Dynamic mode: read the target EntId from the connected "Target" input port.
        // Returns EntId.Invalid when no Target port is connected.
        public OperationResult<EntId> GetDynamicTargetEntity()
        {
            NodePort? targetPort = GetInputPort(TARGET_ENTITY_INPUT_NAME);
            if (targetPort == null || !targetPort.IsConnected)
                return OperationResult<EntId>.Success(EntId.Invalid);
            return Resolve<EntId>(TARGET_ENTITY_INPUT_NAME);
        }

        public OperationResult<object> GetValue(string portName)
        {
            BaseEntityEvent? ev = GetCoreEntityEvent();
            if (ev == null)
                return OperationResult<object>.Failure($"No BaseEntityEvent found in {name}");

            if (fieldCache == null)
            {
                fieldCache = new();
                EventNodeUtils.BuildFieldCache(fieldCache, vsFieldPorts);
            }

            return EventNodeUtils.ReadFromObject(portName, fieldCache, ev);
        }

        // Entity filtering is handled by VSEventListenersSystem via separate maps.
        public override bool CanExecute(VSEventBase _, EntId ownerId) => IsActive;

        // public override void Register(Type _, EntId ownerId)
        //     => base.Register(EventType!, ownerId);

        // public override void DeRegister(Type _, EntId ownerId)
        //     => base.DeRegister(EventType!, ownerId);

#if UNITY_EDITOR
        private void BuildDynamicPorts()
        {
            Type? t = EventType;
            if (t == null) return;
            EventNodeUtils.CreateFieldPorts(t, vsFieldPorts, false);

            // Add or remove the "Target" input port depending on ListenTarget
            if (ListenTarget == VSEntityListenTarget.Dynamic)
            {
                if (GetInputPort(TARGET_ENTITY_INPUT_NAME) == null)
                    AddDynamicInput(typeof(EntId), fieldName: TARGET_ENTITY_INPUT_NAME,
                        typeConstraint: Node.TypeConstraint.Strict,
                        connectionType: Node.ConnectionType.Override);
            }
            else
            {
                NodePort? targetPort = GetInputPort(TARGET_ENTITY_INPUT_NAME);
                if (targetPort != null)
                    RemoveDynamicPort(targetPort);
            }

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
