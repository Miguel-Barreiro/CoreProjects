
using System;
using System.Collections.Generic;
using Core.Events;
using Core.Model;
using Core.Model.Time;
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
        [Inject] private readonly ITimerSystemImplementationInternal ITimerSystem = null!;

        public event Action OnEndFrame;
        
        private bool running = false;
        private bool paused = false;
        private SystemsControllerMode mode = SystemsControllerMode.UNIT_TESTS;
        
        public void Initialize()
        {
            running = false;
        }
        
        public void PauseLoop()
        {
            paused = true;
        }
        
        public void ResumeLoop()
        {
            paused = false;
        }

        public void StartSystem()
        {
            running = true;
            if (mode == SystemsControllerMode.AUTOMATIC)
            {
                StartLoop();
            }
        }

        private async void StartLoop()
        {
            await UniTask.DelayFrame(1);
            running = true;
            
            GameLoopExecuter().Forget();

            async UniTask GameLoopExecuter()
            {
                while (running)
                {
                    await UniTask.DelayFrame(1, PlayerLoopTiming.EarlyUpdate);
                    float deltaTime = Time.deltaTime;
                    if (!paused)
                        ExecuteFrame(deltaTime);
  
                    OnEndFrame?.Invoke();
                }
                Debug.Log($"SystemsController: Stopped"); 
            }
        }

        public void ExecuteFrame(float deltaTime)
        {
            if (!running)
            {
                Debug.Log("ExecuteFrame: asked to run while stopped"); 
                return;
            }
            
            ITimerSystem.Update(deltaTime*1000);
            
#if !UNITY_EDITOR
            try
            {
#endif   
                ExecuteComponentUpdateSystems();
#if !UNITY_EDITOR
            } catch (Exception e)
            {
                Debug.LogError("Error in a component system: " + e);
            }
#endif                
            
#if !UNITY_EDITOR
            try
            {
#endif  
                ExecuteUpdateSystems();
#if !UNITY_EDITOR
            } catch (Exception e)
            {
                Debug.LogError("Error in a system: " + e);
            }
#endif                
            
#if !UNITY_EDITOR
            try
            {
#endif  
                ExecuteEvents();
#if !UNITY_EDITOR
            } catch (Exception e)
            {
                Debug.LogError("Error in an event: " + e);
            }
#endif

            void ExecuteComponentUpdateSystems()
            {
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
            }

            void ExecuteUpdateSystems()
            {
                IEnumerable<IUpdateSystem> allSystemsByInterface = systemsContainer.GetAllSystemsByInterface<IUpdateSystem>();
                foreach (IUpdateSystem system in allSystemsByInterface)
                {
                    if (system.Active)
                    {
                        system.UpdateSystem(deltaTime);
                    }
                }
            }

            void ExecuteEvents()
            {
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
            IEnumerable<BaseEntity> allDestroyedEntities = EntitiesContainer.GetAllDestroyedEntities();
            destroyedEntitiesList.AddRange(allDestroyedEntities);

            while (destroyedEntitiesList.Count > 0)
            {
                EntitiesContainer.ClearDestroyedEntities();

                using CachedList<EntitySystemsContainer.SystemCache> latetList = ListCache<EntitySystemsContainer.SystemCache>.Get();
                using CachedList<EntitySystemsContainer.SystemCache> defaultList = ListCache<EntitySystemsContainer.SystemCache>.Get();
                using CachedList<EntitySystemsContainer.SystemCache> earlytList = ListCache<EntitySystemsContainer.SystemCache>.Get();

                
                foreach (BaseEntity destroyedEntity in destroyedEntitiesList)
                {
                    Type entityType = destroyedEntity.GetType();
                    IEnumerable<Type> components = TypeCache.Get().GetComponentsOf(entityType);
                    foreach (Type componentType in components)
                    {
                        latetList.AddRange(systemsContainer.GetComponentSystemsForDestroyed(componentType, 
                                                                                            SystemPriority.Late));
                        defaultList.AddRange(systemsContainer.GetComponentSystemsForDestroyed(componentType, 
                                                                                            SystemPriority.Default));
                        earlytList.AddRange(systemsContainer.GetComponentSystemsForDestroyed(componentType, 
                                                                                            SystemPriority.Early));
                    }
                    
                    CallOnDestroy(latetList, destroyedEntity);
                    CallOnDestroy(defaultList, destroyedEntity);
                    CallOnDestroy(earlytList, destroyedEntity);

                    earlytList.Clear();
                    defaultList.Clear();
                    latetList.Clear();
                }
                
                allDestroyedEntities = EntitiesContainer.GetAllDestroyedEntities();
                destroyedEntitiesList.Clear();
                destroyedEntitiesList.AddRange(allDestroyedEntities);
            }

            void CallOnDestroy(CachedList<EntitySystemsContainer.SystemCache> systemList, BaseEntity destroyedEntity)
            {
                ARGUMENT[0] = destroyedEntity;
                foreach (EntitySystemsContainer.SystemCache systemCache in systemList)
                {
                    BaseEntitySystem system = systemCache.System;
                    systemCache.CachedOnEntityDestroyedMethod?.Invoke(system, ARGUMENT);
                }
            }
        }

        private void ProcessNewEntities()
        {
            
            //TODO: Optimize this by grouping by component type
            using CachedList<BaseEntity> newEntitiesList = ListCache<BaseEntity>.Get();
            IEnumerable<BaseEntity> newEntities = EntitiesContainer.GetAllNewEntities();
            newEntitiesList.AddRange(newEntities);

            while (newEntitiesList.Count > 0)
            {
                EntitiesContainer.UpgradeCurrentNewEntities();
             
                using CachedList<EntitySystemsContainer.SystemCache> latetList = ListCache<EntitySystemsContainer.SystemCache>.Get();
                using CachedList<EntitySystemsContainer.SystemCache> defaultList = ListCache<EntitySystemsContainer.SystemCache>.Get();
                using CachedList<EntitySystemsContainer.SystemCache> earlytList = ListCache<EntitySystemsContainer.SystemCache>.Get();

                
                foreach (BaseEntity newEntity in newEntitiesList)
                {
                    Type entityType = newEntity.GetType();
                    IEnumerable<Type> components = TypeCache.Get().GetComponentsOf(entityType);
                    foreach (Type componentType in components)
                    {
                        latetList.AddRange(systemsContainer.GetComponentSystemsFor(componentType, SystemPriority.Late));
                        defaultList.AddRange(systemsContainer.GetComponentSystemsFor(componentType, SystemPriority.Default));
                        earlytList.AddRange(systemsContainer.GetComponentSystemsFor(componentType, SystemPriority.Early));
                    }
                    
                    CallOnNew(earlytList, newEntity);
                    CallOnNew(defaultList, newEntity);
                    CallOnNew(latetList, newEntity);

                    earlytList.Clear();
                    defaultList.Clear();
                    latetList.Clear();
                }
                
                newEntities = EntitiesContainer.GetAllNewEntities();
                newEntitiesList.Clear();
                newEntitiesList.AddRange(newEntities);
                
            }
            
            void CallOnNew(CachedList<EntitySystemsContainer.SystemCache> systemList, BaseEntity newEntity)
            {
                ARGUMENT[0] = newEntity;
                foreach (EntitySystemsContainer.SystemCache systemCache in systemList)
                {
                    BaseEntitySystem system = systemCache.System;
                    systemCache.CachedOnNewEntityMethod?.Invoke(system, ARGUMENT);
                }
            }
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