using Core.Model;
using Core.Model.ModelSystems;
using Core.Systems;
using UnityEngine;
using Zenject;

#nullable enable

namespace Core.View
{
    
    public sealed class KineticEntityUpdateViewSystem : OnCreateComponent<KineticComponentData>, 
                                                        OnDestroyComponent<KineticComponentData>, 
                                                        UpdateComponents<KineticComponentData>
    {
        [Inject] private readonly ViewEntitiesContainer ViewEntitiesContainer = null!;

        [Inject] private readonly ComponentContainer<KineticComponentData> KineticEntityComponentContainer = null!;
        
        public void OnCreateComponent(EntId newComponentId)
        {
            ref KineticComponentData newComponent = ref KineticEntityComponentContainer.GetComponent(newComponentId);
            if (newComponent.Prefab == null)
            {
                Debug.LogError($"Prefab not found for entity {newComponent.GetType().Name}({newComponent.ID})");
                return;
            }

            GameObject? newGameObject = ViewEntitiesContainer.Spawn(newComponent.Prefab, newComponent.ID);
            if (newGameObject != null)
            {
                newGameObject.transform.position = new Vector3(newComponent.Position.x, newComponent.Position.y, 0);
            }

        }

        public void OnDestroyComponent(EntId destroyedComponentId)
        {
            EntityViewAtributes? entityViewAtributes = ViewEntitiesContainer.GetEntityViewAtributes(destroyedComponentId);
            if (entityViewAtributes == null)
            {
                Debug.LogError($"view for Entity with id {destroyedComponentId} not found");
                return;
            }

            ViewEntitiesContainer.Destroy(destroyedComponentId);
        }

        public void UpdateComponents(ComponentContainer<KineticComponentData> componentContainer, float deltaTime)
        {
            
            componentContainer.ResetIterator();
            while (componentContainer.MoveNext())
            {
                ref KineticComponentData kineticComponentData = ref componentContainer.GetCurrent();
                EntId entityID = kineticComponentData.ID;
                
                EntityViewAtributes? viewAtributes = ViewEntitiesContainer.GetEntityViewAtributes(entityID);
                if (viewAtributes == null || viewAtributes.GameObject== null)
                {
                    GameObject? newGameObject = ViewEntitiesContainer.Spawn(kineticComponentData.Prefab, entityID);
                    return;
                }

                GameObject entityGameobject = viewAtributes.GameObject;
                entityGameobject.transform.position = new Vector3(kineticComponentData.Position.x, kineticComponentData.Position.y, 0);
            }

        }

        public bool Active { get; set; } = true;
        public SystemGroup Group { get; } = CoreSystemGroups.CoreViewEntitySystemGroup;
    }
}