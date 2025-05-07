
using System;
using System.Collections.Generic;
using Core.Events;
using Core.Model;
using Core.Model.ModelSystems;
using Core.Model.ModelSystems.ComponentSystems;
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
        [Inject] private readonly DataContainersControllerImplementation DataContainersController = null!;

        public event Action OnEndFrame;
        
        private bool running = false;
        private bool paused = false;
        private SystemsControllerMode mode = SystemsControllerMode.UNIT_TESTS;
        
        
        private readonly Dictionary<Type, List<EntId>> DestroyedEntitiesByComponentType = new Dictionary<Type, List<EntId>>();
        private readonly Dictionary<Type, List<EntId>> NewEntitiesByComponentType = new Dictionary<Type, List<EntId>>();

        
        public void Initialize()
        {
            running = false;
            DestroyedEntitiesByComponentType.Clear();
            NewEntitiesByComponentType.Clear();
            foreach (Type componentType in TypeCache.Get().GetAllEntityComponentTypes())
            {
                DestroyedEntitiesByComponentType.Add(componentType, new List<EntId>());
                NewEntitiesByComponentType.Add(componentType, new List<EntId>());
            }
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
        
        private static readonly object[] ARGUMENT_SINGLE = { null };
        private static readonly object[] ARGUMENT_PAIR = { null, null };


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
                ExecuteFrameUpdateSystems();
#if !UNITY_EDITOR
            } catch (Exception e)
            {
                Debug.LogError("Error in a system: " + e);
            }
#endif                
            
            ExecuteEventsAndUpdates();

            
            EntitiesContainer.ClearAllFlushedDeadEntities();
            EntitiesContainer.UpgradeAllFlushedNewEntities();

            
            void ExecuteFrameUpdateSystems()
            {
                IEnumerable<IUpdateSystem> allSystemsByInterface = systemsContainer.GetAllSystemsByInterface<IUpdateSystem>();
                foreach (IUpdateSystem system in allSystemsByInterface)
                {
                    if (system.Active)
                        system.UpdateSystem(deltaTime);
                }
            }

            void ExecuteEventsAndUpdates()
            {
                int loopGard = 0;
                while ( loopGard < 10 && 
                        (
                            eventQueue.EventsCount > 0 || 
                            EntitiesContainer.NewEntitiesCount() > 0 ||
                            EntitiesContainer.destroyedEntitiesCount > 0))
                {
                    loopGard++;
                    
                    //only call update on components the first time
                    ProcessEntitiesEvents(loopGard == 0);
                    
                    ProcessEvents();
                }
                
            }
            
            
            void ProcessEntitiesEvents(bool callUpdate)
            {
                foreach (List<EntId> list in NewEntitiesByComponentType.Values)
                    list.Clear();

                foreach (List<EntId> list in DestroyedEntitiesByComponentType.Values)
                    list.Clear();
                
                //first we optimize by grouping by component type
                foreach (Entity destroyedEntity in EntitiesContainer.GetAllDestroyedEntities())
                {
                    EntId destroyedEntityID = destroyedEntity.ID;
                    
                    Type entityType = destroyedEntity.GetType();
                    IEnumerable<Type> components = TypeCache.Get().GetComponentsOf(entityType);
                    foreach (Type component in components)
                        DestroyedEntitiesByComponentType[component].Add(destroyedEntityID);
                }
                foreach (Entity newEntity in EntitiesContainer.GetAllNewEntities())
                {
                    EntId newEntityID = newEntity.ID;
                    
                    Type entityType = newEntity.GetType();
                    IEnumerable<Type> components = TypeCache.Get().GetComponentsOf(entityType);
                    foreach (Type component in components)
                        NewEntitiesByComponentType[component].Add(newEntityID);
                }
                
                
                EntitiesContainer.FlushCurrentDestroyedEntities();
                EntitiesContainer.FlushCurrentNewEntities();

                
                // then we call the systems by component type
                
                IEnumerable<KeyValuePair<Type, ComponentSystemListenerGroup>> systemsByComponentType = systemsContainer.GetAllEntitySystemsByComponentType();
                foreach ((Type componentType, ComponentSystemListenerGroup listenerGroup) in systemsByComponentType)
                {

                    if(callUpdate)
                        CallComponentUpdate(listenerGroup, componentType);
                    
                    List<EntId> destroyedEntities = DestroyedEntitiesByComponentType[componentType];
                    List<EntId> newEntities = NewEntitiesByComponentType[componentType];
                    
                    foreach (EntId destroyedEntityID in destroyedEntities)
                    {
                        ARGUMENT_SINGLE[0] = destroyedEntityID;
                        foreach (OnDestroyComponentSystemCache systemCache in listenerGroup.OnDestroyEarlierPriority)
                            systemCache.Call(ARGUMENT_SINGLE);
                        foreach (OnDestroyComponentSystemCache systemCache in listenerGroup.OnDestroyDefaultPriority)
                            systemCache.Call(ARGUMENT_SINGLE);
                        foreach (OnDestroyComponentSystemCache systemCache in listenerGroup.OnDestroyLatePriority)
                            systemCache.Call(ARGUMENT_SINGLE);
                    }
                    
                    foreach (EntId newEntityID in newEntities)
                    {
                        ARGUMENT_SINGLE[0] = newEntityID;
                        foreach (OnCreateComponentSystemCache systemCache in listenerGroup.OnCreateEarlierPriority)
                            systemCache.Call(ARGUMENT_SINGLE);
                        foreach (OnCreateComponentSystemCache systemCache in listenerGroup.OnCreateDefaultPriority)
                            systemCache.Call(ARGUMENT_SINGLE);
                        foreach (OnCreateComponentSystemCache systemCache in listenerGroup.OnCreateLatePriority)
                            systemCache.Call(ARGUMENT_SINGLE);
                    }
                }
                
                
                void CallComponentUpdate(ComponentSystemListenerGroup group, Type componentType)
                {

                    object componentContainer = DataContainersController.GetComponentContainer(componentType);
                    ARGUMENT_PAIR[0] = componentContainer;
                    ARGUMENT_PAIR[1] = deltaTime;

                    foreach (UpdateComponentSystemCache systemCache in group.UpdateEarlierPriority)
                    {
                        systemCache.Call(ARGUMENT_PAIR);
                        // if(systemCacheSystem.Active)
                        //     systemCacheSystem.Update(EntitiesContainer, deltaTime);
                    }
                        
                    foreach (UpdateComponentSystemCache systemCache in group.UpdateDefaultPriority)
                    {
                        systemCache.Call(ARGUMENT_PAIR);
                        // if(systemCacheSystem.Active)
                        //     systemCacheSystem.Update(EntitiesContainer, deltaTime);
                    }
                    foreach (UpdateComponentSystemCache systemCache in group.UpdateLatePriority)
                    {
                        systemCache.Call(ARGUMENT_PAIR);
                            
                        // if(systemCacheSystem.Active)
                        //     systemCacheSystem.Update(EntitiesContainer, deltaTime);
                    }
                }
            }
            
        }

        
        private readonly List<BaseEvent> EventsToProcessList = new List<BaseEvent>();
        private void ProcessEvents()
        {
            EventsToProcessList.Clear();
            eventQueue.PopEvents(EventsToProcessList);
            
            foreach (BaseEvent currentEvent in EventsToProcessList)
            {
                
#if !UNITY_EDITOR
                try
                {
#endif  
                    currentEvent.CallPreListenerSystemsInternal();
                    currentEvent.Execute();
                    currentEvent.CallPostListenerSystemsInternal();
                    currentEvent.Dispose();
                    
#if !UNITY_EDITOR
                } catch (Exception e)
                {
                    Debug.LogError($"Error in the event({currentEvent.GetType()}): ");
                    Debug.LogException(e);
                }
#endif                
                
            }
            
            EventsToProcessList.Clear();
        }

        
        
       
        
       
        
        

        // private void ProcessDestroyedEntities()
        // {
        //     //TODO: Optimize this by grouping by component type
        //     using CachedList<Entity> destroyedEntitiesList = ListCache<Entity>.Get();
        //     IEnumerable<Entity> allDestroyedEntities = EntitiesContainer.GetAllDestroyedEntities();
        //     destroyedEntitiesList.AddRange(allDestroyedEntities);
        //
        //     while (destroyedEntitiesList.Count > 0)
        //     {
        //         EntitiesContainer.ClearDestroyedEntities();
        //
        //         // using CachedList<EntitySystemsContainer.SystemCache> latetList = ListCache<EntitySystemsContainer.SystemCache>.Get();
        //         // using CachedList<EntitySystemsContainer.SystemCache> defaultList = ListCache<EntitySystemsContainer.SystemCache>.Get();
        //         // using CachedList<EntitySystemsContainer.SystemCache> earlytList = ListCache<EntitySystemsContainer.SystemCache>.Get();
        //         //
        //         //
        //         // foreach (Entity destroyedEntity in destroyedEntitiesList)
        //         // {
        //         //     Type entityType = destroyedEntity.GetType();
        //         //     IEnumerable<Type> components = TypeCache.Get().GetComponentsOf(entityType);
        //         //     foreach (Type componentType in components)
        //         //     {
        //         //         latetList.AddRange(systemsContainer.GetComponentSystemsForDestroyed(componentType, 
        //         //                                                                             SystemPriority.Late));
        //         //         defaultList.AddRange(systemsContainer.GetComponentSystemsForDestroyed(componentType, 
        //         //                                                                             SystemPriority.Default));
        //         //         earlytList.AddRange(systemsContainer.GetComponentSystemsForDestroyed(componentType, 
        //         //                                                                             SystemPriority.Early));
        //         //     }
        //         //     
        //         //     CallOnDestroy(latetList, destroyedEntity);
        //         //     CallOnDestroy(defaultList, destroyedEntity);
        //         //     CallOnDestroy(earlytList, destroyedEntity);
        //         //
        //         //     earlytList.Clear();
        //         //     defaultList.Clear();
        //         //     latetList.Clear();
        //         // }
        //         //
        //         allDestroyedEntities = EntitiesContainer.GetAllDestroyedEntities();
        //         destroyedEntitiesList.Clear();
        //         destroyedEntitiesList.AddRange(allDestroyedEntities);
        //     }
        //
        //     void CallOnDestroy(CachedList<OnDestroyComponentSystemCache> systemList, Entity destroyedEntity)
        //     {
        //         ARGUMENT_PAIR[0] = destroyedEntity;
        //         foreach (OnDestroyComponentSystemCache systemCache in systemList)
        //         {
        //             systemCache.CachedOnDestroyedMethod?.Invoke(systemCache.System, ARGUMENT_SINGLE);
        //         }
        //     }
        // }

        
        // private void ProcessNewEntities()
        // {
        //     
        //     //TODO: Optimize this by grouping by component type
        //     using CachedList<Entity> newEntitiesList = ListCache<Entity>.Get();
        //     IEnumerable<Entity> newEntities = EntitiesContainer.GetAllNewEntities();
        //     newEntitiesList.AddRange(newEntities);
        //
        //     while (newEntitiesList.Count > 0)
        //     {
        //         EntitiesContainer.UpgradeCurrentNewEntities();
        //      
        //
        //         
        //         foreach (Entity newEntity in newEntitiesList)
        //         {
        //             Type entityType = newEntity.GetType();
        //             IEnumerable<Type> components = TypeCache.Get().GetComponentsOf(entityType);
        //             foreach (Type componentType in components)
        //             {
        //                 latetList.AddRange(systemsContainer.GetOnCreateComponentSystemsFor(componentType, SystemPriority.Late));
        //                 defaultList.AddRange(systemsContainer.GetOnCreateComponentSystemsFor(componentType, SystemPriority.Default));
        //                 earlytList.AddRange(systemsContainer.GetOnCreateComponentSystemsFor(componentType, SystemPriority.Early));
        //             }
        //             
        //             CallOnNew(earlytList, newEntity);
        //             CallOnNew(defaultList, newEntity);
        //             CallOnNew(latetList, newEntity);
        //
        //             earlytList.Clear();
        //             defaultList.Clear();
        //             latetList.Clear();
        //         }
        //         
        //         newEntities = EntitiesContainer.GetAllNewEntities();
        //         newEntitiesList.Clear();
        //         newEntitiesList.AddRange(newEntities);
        //         
        //     }
        //     
        //     void CallOnNew(CachedList<OnCreateComponentSystemCache> systemList, Entity newEntity)
        //     {
        //         ARGUMENT_SINGLE[0] = newEntity;
        //         foreach (OnCreateComponentSystemCache systemCache in systemList)
        //             systemCache.CachedOnCreateMethod?.Invoke(systemCache.System, ARGUMENT_SINGLE);
        //     }
        // }

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