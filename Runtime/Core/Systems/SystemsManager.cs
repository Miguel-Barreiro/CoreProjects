using UnityEngine;
using Zenject;

namespace Core.Systems
{
    public class SystemsManager : MonoBehaviour
    {
        [Inject] private readonly SystemsContainer systemsContainer = null!;
        public SystemsContainer SystemsContainer => systemsContainer;
        
        
    }
}