#nullable enable

using System;
using System.Collections.Generic;
using Core.Events;
using Core.Utils;
using Core.VSEngine.Nodes;
using Core.VSEngine.Nodes.Events;
using UnityEngine;
using UnityEngine.Serialization;
using XNode;

namespace Core.VSEngine
{
    
    [Node.NodeWidth(220)]
    [Node.CreateNodeMenu(VSNodeMenuNames.EVENTS_MENU+"/[Write] Event", order = VSNodeMenuNames.IMPORTANT)]
    [Node.NodeTint(VSNodeMenuNames.WRITE_NODES_TINT)]
    public sealed class EventWriteNode : BasicFlowNode
    {
        [SerializeField, HideInInspector]
        protected List<VSFieldPort> vsFieldPorts = new List<VSFieldPort>();

        [SerializeField] private EventSerializedType EventType = new EventSerializedType();
        
        
        protected override void Action()
        {
            IBaseEvent? vsEvent = GetCoreEvent();
            if (vsEvent == null)
            {
                Debug.LogError($"no event found for node {name} in {graph.name}");
                return;
            }

            EventNodeUtils.Write( this, vsFieldPorts, vsEvent, EventType.GetParsedType());
        }

        public override OperationResult<object> GetValue(string portName)
        {
            return OperationResult<object>.Failure($"{name} [{GetType().Name}] in {graph.name} should not have any value output ports");
        }

#if UNITY_EDITOR
        
        private void BuildDynamicPorts()
        {
            Type? eventType = EventType.GetParsedType();
            if (eventType == null)
            {
                // Debug.LogError($"no event type set found for node {name} in {graph.name}");
                return;
            }

            EventNodeUtils.CreateFieldPorts(EventType.GetParsedType(), vsFieldPorts, true);
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