
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
        
        
        private readonly Dictionary<Type, List<EntId>> DestroyedEntitiesByComponentDataType = new Dictionary<Type, List<EntId>>();
        private readonly Dictionary<Type, List<EntId>> NewEntitiesByComponentDataType = new Dictionary<Type, List<EntId>>();

        
        public void Initialize()
        {
            running = false;
            DestroyedEntitiesByComponentDataType.Clear();
            NewEntitiesByComponentDataType.Clear();
            foreach (Type componentDataType in TypeCache.Get().GetAllComponentDataTypes())
            {
                DestroyedEntitiesByComponentDataType.Add(componentDataType, new List<EntId>());
                NewEntitiesByComponentDataType.Add(componentDataType, new List<EntId>());
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
            EntitiesContainer.ProcessAllFlushedEntities();
            
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
                
                ProcessAllEntitiesEvents();
                
                bool hasEntitiesToProcess = EntitiesContainer.newEntitiesCount > 0 || EntitiesContainer.destroyedEntitiesCount > 0;
                int loopGard = 0;
                while ( loopGard < 10 && 
                        (eventQueue.EventsCount > 0 || hasEntitiesToProcess))
                {
                    if(hasEntitiesToProcess)
                        ProcessDestroyAndCreateEntitiesEvents();
    
                    if(eventQueue.EventsCount > 0)
                        ProcessGlobalEvents();
                    
                    hasEntitiesToProcess = EntitiesContainer.newEntitiesCount > 0 || EntitiesContainer.destroyedEntitiesCount > 0;
                    loopGard++;
                }
                
            }


            #region utility

            void ProcessAllEntitiesEvents()
            {
                GroupNewAndDestroyEntitiesByComponent();
                
                // then we call the systems by component type
                
                IEnumerable<KeyValuePair<Type, ComponentSystemListenerGroup>> systemsByComponentType = systemsContainer.GetAllEntitySystemsByComponentDataType();
                foreach ((Type componentDataType, ComponentSystemListenerGroup listenerGroup) in systemsByComponentType)
                {

                    CallComponentUpdate(listenerGroup, componentDataType);

                    if (DestroyedEntitiesByComponentDataType.TryGetValue(componentDataType, out List<EntId> destroyedEntities))
                    {
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
                    }

                    if (NewEntitiesByComponentDataType.TryGetValue(componentDataType, out List<EntId> newEntities))
                    {
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
            
            void ProcessDestroyAndCreateEntitiesEvents()
            {
                GroupNewAndDestroyEntitiesByComponent();
                
                // then we call the systems by component type
                
                IEnumerable<KeyValuePair<Type, ComponentSystemListenerGroup>> systemsByComponentType = systemsContainer.GetAllEntitySystemsByComponentDataType();
                foreach ((Type componentDataType, ComponentSystemListenerGroup listenerGroup) in systemsByComponentType)
                {
                    
                    if (DestroyedEntitiesByComponentDataType.TryGetValue(componentDataType, out List<EntId> destroyedEntities))
                    {
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
                    }

                    if (NewEntitiesByComponentDataType.TryGetValue(componentDataType, out List<EntId> newEntities))
                    {
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
                }
            }
            
            void GroupNewAndDestroyEntitiesByComponent()
            {
                foreach (List<EntId> list in NewEntitiesByComponentDataType.Values)
                    list.Clear();

                foreach (List<EntId> list in DestroyedEntitiesByComponentDataType.Values)
                    list.Clear();
                    
                //first we optimize by grouping by component type
                foreach (Entity destroyedEntity in EntitiesContainer.GetAllDestroyedEntities())
                {
                    Type entityType = destroyedEntity.GetType();
                    IEnumerable<Type> componentDatas = TypeCache.Get().GetComponentDatasOfEntityType(entityType);
                    foreach (Type componentData in componentDatas)
                        DestroyedEntitiesByComponentDataType[componentData].Add(destroyedEntity.ID);
                }
                foreach (Entity newEntity in EntitiesContainer.GetAllNewEntities())
                {
                    EntId newEntityID = newEntity.ID;
                    Type entityType = newEntity.GetType();
                    IEnumerable<Type> componentDatas = TypeCache.Get().GetComponentDatasOfEntityType(entityType);
                    foreach (Type componentData in componentDatas)
                        NewEntitiesByComponentDataType[componentData].Add(newEntityID);
                }
                    
                    
                EntitiesContainer.FlushCurrentDestroyedEntities();
                EntitiesContainer.FlushCurrentNewEntities();
            }

            #endregion
        }
        
        
        
        private readonly List<BaseEvent> EventsToProcessList = new List<BaseEvent>();
        private void ProcessGlobalEvents()
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