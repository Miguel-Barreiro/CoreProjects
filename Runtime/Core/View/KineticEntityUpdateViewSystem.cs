using Core.Model;
using Core.Model.ModelSystems;
using Core.Systems;
using UnityEngine;
using Zenject;

#nullable enable

namespace Core.View
{
    
    public sealed class KineticEntityUpdateViewSystem : OnCreateComponent<KineticEntityData>, 
                                                        OnDestroyComponent<KineticEntityData>, 
                                                        UpdateComponents<KineticEntityData>
    {
        [Inject] private readonly ViewEntitiesContainer ViewEntitiesContainer = null!;

        [Inject] private readonly ComponentContainer<KineticEntityData> KineticEntityComponentContainer = null!;
        
        public void OnCreateComponent(EntId newComponentId)
        {
            ref KineticEntityData newComponent = ref KineticEntityComponentContainer.GetComponent(newComponentId);
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

        public void UpdateComponents(ComponentContainer<KineticEntityData> componentContainer, float deltaTime)
        {
            
            componentContainer.ResetIterator();
            while (componentContainer.MoveNext())
            {
                ref KineticEntityData kineticEntityData = ref componentContainer.GetCurrent();
                EntId entityID = kineticEntityData.ID;
                
                EntityViewAtributes? viewAtributes = ViewEntitiesContainer.GetEntityViewAtributes(entityID);
                if (viewAtributes == null || viewAtributes.GameObject== null)
                {
                    GameObject? newGameObject = ViewEntitiesContainer.Spawn(kineticEntityData.Prefab, entityID);
                    return;
                }

                GameObject entityGameobject = viewAtributes.GameObject;
                entityGameobject.transform.position = new Vector3(kineticEntityData.Position.x, kineticEntityData.Position.y, 0);
            }

        }

        public bool Active { get; set; } = true;
        public SystemGroup Group { get; } = CoreSystemGroups.CoreViewEntitySystemGroup;
    }
}