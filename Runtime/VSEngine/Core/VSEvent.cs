using System;
using System.Collections.Generic;
using Core.Events;
using UnityEngine;
using VSEngine.Core;

namespace Core.VSEngine
{

    [Serializable]
    public abstract class VSEventBase
    {
        private bool isPropagating = true;

        public bool IsPropagating => isPropagating;
        
        
        public void StopPropagation()
        {
            Debug.Log($"VS: Stopped propagation on event {this.GetType().Name}");
            isPropagating = false;
        }
    }

    [Serializable]
    public abstract class VSEvent<T> : VSEventBase
        where T : VSEvent<T>, new()
    {
        [SerializeField]
        public readonly T VSEventData;
        public T EventData => VSEventData;
        
    }


    // public abstract class VSEventData
    // {
    //     [VSField]
    //     public List<Tag> Tags = new List<Tag>();
    //
    //     public void AddTag(Tag tag)
    //     {
    //         if (!Tags.Contains(tag))
    //         {
    //             Tags.Add(tag);
    //         }
    //     }
    //     
    //     public bool ContainsTag(Tag tag)
    //     {
    //         return Tags.Contains(tag);
    //     }
    // }
}