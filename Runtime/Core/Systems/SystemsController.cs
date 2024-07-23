using System;
using System.Collections.Generic;
using Core.Events;
using Core.Model;
using Core.Utils.CachedDataStructures;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Core.Systems
{
    public sealed class SystemsController : MonoBehaviour, IInitSystem, IStartSystem
    {
        [Inject] private readonly SystemsContainer systemsContainer = null!;
        [Inject] private readonly EntityLifetimeManager entityLifetimeManager = null!;
        [Inject] private readonly EventQueue eventQueue = null!;

        private bool initialized = false;
        
        public void Initialize()
        {
            initialized = false;
        }

        public void StartSystem()
        {
            SetInitialized();
        }

        private async void SetInitialized()
        {
            await UniTask.DelayFrame(1);
            initialized = true;
        }

        void Update()
        {
            if (!initialized)
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            IEnumerable<(Type, EntitySystemsContainer.SystemListenerGroup)> systemsByComponentType = systemsContainer.GetAllEntitySystemsByComponentType();
            foreach ((Type _, EntitySystemsContainer.SystemListenerGroup group) in systemsByComponentType)
            {

                foreach (EntitySystemsContainer.SystemCache systemCache in group.EarlierPriority)
                {
                    BaseEntitySystem systemCacheSystem = systemCache.System;
                    
                    if(systemCacheSystem.Active)
                        systemCacheSystem.Update(entityLifetimeManager, deltaTime);
                }
                
                foreach (EntitySystemsContainer.SystemCache systemCache in group.DefaultPriority)
                {
                    BaseEntitySystem systemCacheSystem = systemCache.System;
                    
                    if(systemCacheSystem.Active)
                        systemCacheSystem.Update(entityLifetimeManager, deltaTime);
                }
                
                foreach (EntitySystemsContainer.SystemCache systemCache in group.LatePriority)
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

            int loopGard = 0; 
            while ( loopGard < 10 && 
                    (
                        eventQueue.EventsCount > 0 || 
                        entityLifetimeManager.NewEntitiesCount() > 0 ||
                        entityLifetimeManager.DestroyedEntitiesCount > 0))
            {
                loopGard++;
                ProcessEvents();
                ProcessDestroyedEntities();
                ProcessNewEntities();
            }
        }

        private void ProcessEvents()
        {
            IEnumerable<BaseEvent> events = eventQueue.PopEvents();
            using CachedList<BaseEvent> eventList = ListCache<BaseEvent>.Get(events);
            
            foreach (BaseEvent currentEvent in eventList)
            {
                currentEvent.CallListenerSystemsInternal();
                currentEvent.Execute();
            }
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
                
                foreach (BaseEntity destroyedEntity in destroyedEntitiesList)
                {
                    Type entityType = destroyedEntity.GetType();
                    IEnumerable<Type> components = TypeCache.Get().GetComponentsOf(entityType);
                    foreach (Type componentType in components)
                    {
                        IEnumerable<EntitySystemsContainer.SystemCache> componentSystems = systemsContainer.GetComponentSystemsFor(componentType);
                        foreach (EntitySystemsContainer.SystemCache systemCache in componentSystems)
                        {
                            BaseEntitySystem system = systemCache.System;
                            ARGUMENT[0] = destroyedEntity;
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
                    IEnumerable<Type> components = TypeCache.Get().GetComponentsOf(entityType);
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