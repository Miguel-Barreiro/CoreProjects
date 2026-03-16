using System;
using Core.Model;
using Core.VSEngine.Systems;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Core.VSEngine
{
    public abstract class BaseEventListenNode : ExecutableNode
    {

        [Inject] private readonly VSEventListenersSystem VSEventListenersSystem = null!;

        
        [SerializeField]
        private bool active = true;
        public bool IsActive => active;
        
        [Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict), SerializeField]
        private Control? Continue = null;


        
        public override void Execute()
        {
            ContinueWith(nameof(Continue));
        }

        // public virtual void Register(WorldEngineGlobal engineGlobal, Type eventType, EntId ownerId)
        // public virtual void Register(Type eventType, EntId ownerId)
        // {
        //     if (!this.IsActive)
        //     {
        //         return;
        //     }
        //
        //     VSEventListenersSystem.AddListener(ownerId, eventType, this);
        //     
        //     // WorldState worldState = engineGlobal.WorldState;
        //     // EventListenersDb listenersDb = worldState.GetEventListenersDb();
        //     // listenersDb.AddListener(ownerId, eventType, this);
        // }

        public virtual void DeRegister(Type eventType, EntId ownerId)
        {
            VSEventListenersSystem.RemoveListener(ownerId, eventType, this);
        } 

        public virtual bool CanExecute(VSEventBase vsEvent, EntId ownerId)
        {
            return IsActive;
        }

    }
}