using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Core.Systems
{
    public sealed class SystemsController : MonoBehaviour, IInitSystem
    {
        [Inject] private readonly SystemsContainer systemsContainer = null!;

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
                    system.Update();
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