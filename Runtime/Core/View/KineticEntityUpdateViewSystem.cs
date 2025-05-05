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
                                                        UpdateComponent<IKineticEntityData>
    {
        [Inject] private readonly ViewEntitiesContainer ViewEntitiesContainer = null!;
        [Inject] private readonly ComponentContainer<IKineticEntityData> ComponentContainer = null!;

        public void OnCreateComponent(ref IKineticEntityData newComponent)  
        {
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

        public void OnDestroyComponent(ref IKineticEntityData destroyedComponent)
        {
            EntityViewAtributes? entityViewAtributes = ViewEntitiesContainer.GetEntityViewAtributes(destroyedComponent.ID);
            if (entityViewAtributes == null)
            {
                Debug.LogError($"view for Entity with id {destroyedComponent.ID} not found");
                return;
            }

            ViewEntitiesContainer.Destroy(destroyedComponent.ID);
        }

        public void UpdateComponents(float deltaTime)
        {
            
            ComponentContainer.ResetIterator();
            while (ComponentContainer.MoveNext())
            {
                ref IKineticEntityData kineticEntityData = ref ComponentContainer.GetCurrent();
                
                EntityViewAtributes? viewAtributes = ViewEntitiesContainer.GetEntityViewAtributes(kineticEntityData.ID);
                if (viewAtributes == null || viewAtributes.GameObject== null)
                {
                    GameObject? newGameObject = ViewEntitiesContainer.Spawn(kineticEntityData.Prefab, kineticEntityData.ID);
                    return;
                }

                GameObject entityGameobject = viewAtributes.GameObject;
                entityGameobject.transform.position = new Vector3(kineticEntityData.Position.x, kineticEntityData.Position.y, 0);
            }

        }

        // public override SystemGroup Group { get; } = CoreSystemGroups.CoreViewEntitySystemGroup;
    }
}