using System.Collections.Generic;
using Core.Model;
using UnityEngine;
using Zenject;

namespace Core.Systems
{
    public sealed class SystemsController : MonoBehaviour, IInitSystem
    {
        [Inject] private readonly SystemsContainer systemsContainer = null!;
        [Inject] private readonly EntityLifetimeManager entityLifetimeManager = null!;
        
        private bool initialized = false;
        void Update()
        {
            if (!initialized)
            {
                return;
            }

            IEnumerable<IUpdateSystem> allSystemsByInterface = systemsContainer.GetAllSystemsByInterface<IUpdateSystem>();
            foreach (IUpdateSystem system in allSystemsByInterface)
            {
                if (system.Active)
                {
                    system.UpdateSystem(Time.deltaTime);
                }
            }

            IEnumerable<BaseComponentSystem> componentSystems = systemsContainer.GetAllComponentSystems();
            foreach (BaseComponentSystem componentSystem in componentSystems)
            {
                if (componentSystem.Active)
                {
                    componentSystem.Update(entityLifetimeManager, Time.deltaTime);
                }
            }
            
            

        }


        public void Initialize()
        {
            initialized = false;
        }

        public void Start()
        {
            initialized = true;
        }
    }
}