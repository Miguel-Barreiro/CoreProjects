#nullable enable

using System.Collections.Generic;
using UnityEngine;
using VSEngine;
using XNode;

namespace Core.VSEngine
{
    [Node.NodeTint("#24173b")]
    public abstract class EventWriteNode<T> : BasicFlowNode
        where T : VSEventBase
    {
        [SerializeField, HideInInspector]
        protected List<VSFieldPort> vsFieldPorts = new List<VSFieldPort>();
        
        protected override void Action()
        {
            T? vsEvent = GetEvent<T>();
            if (vsEvent == null)
            {
                Debug.LogError($"no event({typeof(T).Name}) found for node {name} in {graph.name}");
                return;
            }

            EventNodeUtils.Write<T>( this, vsFieldPorts, vsEvent);
        }
        
#if UNITY_EDITOR
        
        private void BuildDynamicPorts()
        {
            EventNodeUtils.CreateFieldPorts<T>(vsFieldPorts, true);
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