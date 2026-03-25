using System.Runtime.InteropServices;
using Core.Model.Data;
using Core.Model.ModelSystems;
using Core.Systems;
using Core.Utils.CachedDataStructures;
using Core.VSEngine;
using Core.VSEngine.Nodes.Events;
using Core.VSEngine.Systems;
using Zenject;

namespace Core.Model
{
    public sealed class VSAbility : Entity, 
                                    IHierarchyEntity, 
                                    IVSAbility
    {

        public VSAbility(EntId ownerID, VSAbilityDataConfig dataConfig) : base() 
        {
            var hierarchySystem = GetSystem<IEntityHierarchySystem>();
            hierarchySystem.AddChild(ownerID, ID);
            
            ref VSAbilityData abilityData = ref GetComponent<VSAbilityData>();
            abilityData.dataConfig = dataConfig;
        }
    }

    [StructLayout(LayoutKind.Auto)]
    public struct VSAbilityData : IComponentData
    {
        public EntId ID { get; set; }
        public VSAbilityDataConfig dataConfig;

        public void Init() { }
    }

    public interface IVSAbility : Component<VSAbilityData> { }
    
    
    public sealed class VSAbilitySystem : OnDestroyComponent<VSAbilityData>, OnCreateComponent<VSAbilityData>
    {

        [Inject] private readonly BasicCompContainer<VSAbilityData> VSAbilityContainer = null!;
        [Inject] private readonly VSEventListenersSystem VSEventListenersSystem = null!;

        public void OnCreateComponent(EntId newComponentId)
        {
            ref VSAbilityData abilityData = ref VSAbilityContainer.GetComponent(newComponentId);

            using CachedList<EventListenNode> eventNodes = ListCache<EventListenNode>.Get();
            using CachedList<EntityEventListenNode> entityEventListenNodes = ListCache<EntityEventListenNode>.Get();

            foreach (ActionGraph actionGraph in abilityData.dataConfig.ActionGraphs)
                ActionGraphUtil.FindListenersFromGraph(actionGraph, eventNodes, entityEventListenNodes);

            foreach (EntityEventListenNode entityEventListenNode in entityEventListenNodes)
                VSEventListenersSystem.AddListener(newComponentId, entityEventListenNode.EventType, entityEventListenNode);

            foreach (EventListenNode eventListenNode in eventNodes)
                VSEventListenersSystem.AddListener(newComponentId, eventListenNode.EventType, eventListenNode);
            
        }

        public void OnDestroyComponent(EntId destroyedComponentId)
        {
            ref VSAbilityData abilityData = ref VSAbilityContainer.GetComponent(destroyedComponentId);

            using CachedList<EventListenNode> eventNodes = ListCache<EventListenNode>.Get();
            using CachedList<EntityEventListenNode> entityEventListenNodes = ListCache<EntityEventListenNode>.Get();

            foreach (ActionGraph actionGraph in abilityData.dataConfig.ActionGraphs)
                ActionGraphUtil.FindListenersFromGraph(actionGraph, eventNodes, entityEventListenNodes);

            foreach (EntityEventListenNode entityEventListenNode in entityEventListenNodes)
                VSEventListenersSystem.RemoveListener(destroyedComponentId, entityEventListenNode.EventType, entityEventListenNode);

            foreach (EventListenNode eventListenNode in eventNodes)
                VSEventListenersSystem.RemoveListener(destroyedComponentId, eventListenNode.EventType, eventListenNode);

        }

        public bool Active { get; set; } = true;
        public SystemGroup Group { get; } = CoreSystemGroups.CoreSystemGroup;
    }
}
