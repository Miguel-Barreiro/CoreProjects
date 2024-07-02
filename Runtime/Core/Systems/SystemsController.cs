using System;
using System.Collections.Generic;
using Core.Model;
using Core.Utils.CachedDataStructures;
using UnityEngine;
using Zenject;

namespace Core.Systems
{
    public sealed class SystemsController : MonoBehaviour, IInitSystem
    {
        [Inject] private readonly SystemsContainer systemsContainer = null!;
        [Inject] private readonly EntityLifetimeManager entityLifetimeManager = null!;
        [Inject] private readonly TypeCache typeCache = null!;
        
        private bool initialized = false;
        
        public void Initialize()
        {
            initialized = false;
        }

        public void Start()
        {
            initialized = true;
        }
        
        void Update()
        {
            if (!initialized)
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            IEnumerable<(Type, List<EntitySystemsContainer.SystemCache>)> systemsByComponentType = systemsContainer.GetAllEntitySystemsByComponentType();
            foreach ((Type _, List<EntitySystemsContainer.SystemCache> systemCaches) in systemsByComponentType)
            {
                foreach (EntitySystemsContainer.SystemCache systemCache in systemCaches)
                {
                    BaseEntitySystem systemCacheSystem = systemCache.System;
                    
                    if(systemCacheSystem.Active)
                        systemCacheSystem.Update(entityLifetimeManager, deltaTime);
                }
            }
            
            IEnumerable<IUpdateSystem> allSystemsByInterface = systemsContainer.GetAllSystemsByInterface<IUpdateSystem>();
            foreach (IUpdateSystem system in allSystemsByInterface)
            {
                if (system.Active)
                {
                    system.UpdateSystem(deltaTime);
                }
            }

            ProcessDestroyedEntities();
            ProcessNewEntities();
        }

        
        
        private static readonly object[] ARGUMENT = { null };
        private void ProcessDestroyedEntities()
        {
            //TODO: Optimize this by grouping by component type
            using CachedList<BaseEntity> destroyedEntitiesList = ListCache<BaseEntity>.Get();
            do
            {
                IEnumerable<BaseEntity> allDestroyedEntities = entityLifetimeManager.GetAllDestroyedEntities();
                destroyedEntitiesList.Clear();
                destroyedEntitiesList.AddRange(allDestroyedEntities);
                entityLifetimeManager.ClearDestroyedEntities();
                
                foreach (BaseEntity newEntity in destroyedEntitiesList)
                {
                    Type entityType = newEntity.GetType();
                    IEnumerable<Type> components = typeCache.GetComponentsOf(entityType);
                    foreach (Type componentType in components)
                    {
                        IEnumerable<EntitySystemsContainer.SystemCache> componentSystems = systemsContainer.GetComponentSystemsFor(componentType);
                        foreach (EntitySystemsContainer.SystemCache systemCache in componentSystems)
                        {
                            BaseEntitySystem system = systemCache.System;
                            ARGUMENT[0] = newEntity;
                            systemCache.CachedOnEntityDestroyedMethod?.Invoke(system, ARGUMENT);
                        }
                    }
                }
                
            } while (destroyedEntitiesList.Count > 0);
        }

        private void ProcessNewEntities()
        {
            
            //TODO: Optimize this by grouping by component type
            using CachedList<BaseEntity> newEntitiesList = ListCache<BaseEntity>.Get();
            do
            {
                IEnumerable<BaseEntity> newEntities = entityLifetimeManager.GetAllNewEntities();
                newEntitiesList.Clear();
                newEntitiesList.AddRange(newEntities);
                entityLifetimeManager.UpgradeCurrentNewEntities();
                
                foreach (BaseEntity newEntity in newEntitiesList)
                {
                    Type entityType = newEntity.GetType();
                    IEnumerable<Type> components = typeCache.GetComponentsOf(entityType);
                    foreach (Type componentType in components)
                    {
                        IEnumerable<EntitySystemsContainer.SystemCache> componentSystems = systemsContainer.GetComponentSystemsFor(componentType);
                        foreach (EntitySystemsContainer.SystemCache systemCache in componentSystems)
                        {
                            BaseEntitySystem system = systemCache.System;
                            ARGUMENT[0] = newEntity;
                            systemCache.CachedOnNewEntityMethod?.Invoke(system, ARGUMENT);
                        }
                    }
                }
                
            } while (newEntitiesList.Count > 0);
        }


    }
}