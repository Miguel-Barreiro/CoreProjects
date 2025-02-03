
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Events;
using Core.Model;
using Core.Utils.CachedDataStructures;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Core.Systems
{

    public interface ISystemsController
    {
        public void ExecuteFrame(float deltaTime);
    }

    public sealed class SystemsController : IInitSystem, IStartSystem, ISystemsController
    {
        [Inject] private readonly SystemsContainer systemsContainer = null!;
        [Inject] private readonly EntitiesContainer EntitiesContainer = null!;
        [Inject] private readonly EventQueue eventQueue = null!;

        private bool running = false;
        private SystemsControllerMode mode = SystemsControllerMode.UNIT_TESTS;
        
        public void Initialize()
        {
            running = false;
        }

        public void StartSystem()
        {
            StartLoop();
        }

        private async void StartLoop()
        {
            await UniTask.DelayFrame(1);
            running = true;
            if (mode == SystemsControllerMode.AUTOMATIC)
            {
                GameLoopExecuter().Forget();
            }

            async UniTask GameLoopExecuter()
            {
                while (running)
                {
                    await UniTask.DelayFrame(1, PlayerLoopTiming.EarlyUpdate);
                    float deltaTime = Time.deltaTime;
                    ExecuteFrame(deltaTime);
                }
            }
        }

        public void ExecuteFrame(float deltaTime)
        {
            if (!running)
            {
                return;
            }

            IEnumerable<(Type, EntitySystemsContainer.SystemListenerGroup)> systemsByComponentType = systemsContainer.GetAllEntitySystemsByComponentType();
            foreach ((Type _, EntitySystemsContainer.SystemListenerGroup group) in systemsByComponentType)
            {

                foreach (EntitySystemsContainer.SystemCache systemCache in group.UpdateEarlierPriority)
                {
                    BaseEntitySystem systemCacheSystem = systemCache.System;
                    
                    if(systemCacheSystem.Active)
                        systemCacheSystem.Update(EntitiesContainer, deltaTime);
                }
                
                foreach (EntitySystemsContainer.SystemCache systemCache in group.UpdateDefaultPriority)
                {
                    BaseEntitySystem systemCacheSystem = systemCache.System;
                    
                    if(systemCacheSystem.Active)
                        systemCacheSystem.Update(EntitiesContainer, deltaTime);
                }
                
                foreach (EntitySystemsContainer.SystemCache systemCache in group.UpdateLatePriority)
                {
                    BaseEntitySystem systemCacheSystem = systemCache.System;
                    
                    if(systemCacheSystem.Active)
                        systemCacheSystem.Update(EntitiesContainer, deltaTime);
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
                        EntitiesContainer.NewEntitiesCount() > 0 ||
                        EntitiesContainer.DestroyedEntitiesCount > 0))
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
                currentEvent.Dispose();
            }
        }


        private static readonly object[] ARGUMENT = { null };

        private void ProcessDestroyedEntities()
        {
            //TODO: Optimize this by grouping by component type
            using CachedList<BaseEntity> destroyedEntitiesList = ListCache<BaseEntity>.Get();
            do
            {
                IEnumerable<BaseEntity> allDestroyedEntities = EntitiesContainer.GetAllDestroyedEntities();
                destroyedEntitiesList.Clear();
                destroyedEntitiesList.AddRange(allDestroyedEntities);
                EntitiesContainer.ClearDestroyedEntities();
                
                foreach (BaseEntity destroyedEntity in destroyedEntitiesList)
                {
                    Type entityType = destroyedEntity.GetType();
                    IEnumerable<Type> components = TypeCache.Get().GetComponentsOf(entityType);
                    foreach (Type componentType in components)
                    {
                        IEnumerable<EntitySystemsContainer.SystemCache> componentSystems = systemsContainer.GetComponentSystemsForDestroyed(componentType);
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
                IEnumerable<BaseEntity> newEntities = EntitiesContainer.GetAllNewEntities();
                newEntitiesList.Clear();
                newEntitiesList.AddRange(newEntities);
                EntitiesContainer.UpgradeCurrentNewEntities();
                
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

        public enum SystemsControllerMode
        {
            AUTOMATIC, 
            UNIT_TESTS
        }

        public void SetMode(SystemsControllerMode mode)
        {
            this.mode = mode;
        }
    }
}