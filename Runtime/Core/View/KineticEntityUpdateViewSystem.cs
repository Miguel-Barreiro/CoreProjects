using Core.Model;
using Core.Model.ModelSystems;
using Core.Systems;
using UnityEngine;
using Zenject;

#nullable enable

namespace Core.View
{
    
    public sealed class KineticEntityUpdateViewSystem : OnCreateComponent<IKineticEntityData>, 
                                                        OnDestroyComponent<IKineticEntityData>, 
                                                        UpdateComponents<IKineticEntityData>
    {
        [Inject] private readonly ViewEntitiesContainer ViewEntitiesContainer = null!;

        [Inject] private readonly ComponentContainer<IKineticEntityData> KineticEntityComponentContainer = null!;
        
        public void OnCreateComponent(EntId newComponentId)
        {
            ref IKineticEntityData newComponent = ref KineticEntityComponentContainer.GetComponent(newComponentId);
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

        public void UpdateComponents(ComponentContainer<IKineticEntityData> componentContainer, float deltaTime)
        {
            
            componentContainer.ResetIterator();
            while (componentContainer.MoveNext())
            {
                ref IKineticEntityData kineticEntityData = ref componentContainer.GetCurrent();
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