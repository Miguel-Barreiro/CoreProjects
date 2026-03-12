#nullable enable

using System.Collections.Generic;
using Core.Utils;
using UnityEngine;
using VSEngine;
using XNode;

namespace Core.VSEngine
{
    [Node.NodeTint("#194d33")]
    public class EventListenNode<T> : BaseEventListenNode, IValueNode
        where T : VSEventBase
    {

        [SerializeField, HideInInspector]
        private List<VSFieldPort> vsFieldPorts = new List<VSFieldPort>();
        
        
        private Dictionary<string, VSFieldPort>? fieldCache = null;
        
        public OperationResult<object> GetValue(string portName)
        {
            
            T vsEvent= GetEvent<T>();
            if (vsEvent == null)
            {
                string message = $"no event({typeof(T).Name}) found for node {name} in {graph.name}";
                Debug.LogError(message);
                return OperationResult<object>.Failure(message);
            }
            
            if(fieldCache == null)
            {
                fieldCache = new();
                EventNodeUtils.BuildFieldCache(fieldCache, vsFieldPorts);
            }

            return EventNodeUtils.Read<T>(portName, fieldCache, vsEvent);
        }

#if UNITY_EDITOR
        
        private void BuildDynamicPorts()
        {
            EventNodeUtils.CreateFieldPorts<T>(vsFieldPorts, false);
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