using System.Collections.Generic;
using Core.Systems;
using Core.Utils;
using Core.Utils.CachedDataStructures;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

#nullable enable

namespace Core.Initialization
{
    public interface ScenesController
    {
        // public UniTask AddScene(string sceneName);
        public UniTask<OperationResult> SwitchScene(string sceneName);
        // public UniTask RemoveScene(string sceneName);
    }


    public sealed class ScenesControllerImplemention : IInitSystem, ScenesController
    {
        [Inject] private readonly SystemsController systemsController = null!;
        [Inject] private readonly Bootstrapper Bootstrapper = null!;
        [Inject] private readonly SystemsController SystemsController = null!;
        
        private List<string> currentSceneNames = new List<string>();
        
        #region Public

        // public UniTask AddScene(string sceneName)
        // {
        //     return UniTask.CompletedTask;
        // }

        private UniTaskCompletionSource<OperationResult>? endOfFrameTask = null;
        private UniTask<OperationResult>? loadingSceneTask = null;
        private string currentLoadingSceneName = NO_SCENE_LOADING;
        private const string NO_SCENE_LOADING = "NONE";
        
        public async UniTask<OperationResult> SwitchScene(string sceneName)
        {
            if(loadingSceneTask != null || endOfFrameTask != null)
            {
                string sceneIsAlreadyLoadingMessage = $"Cant load scene({sceneName}) because scene({currentLoadingSceneName}) is already loading.";
                Debug.LogError(sceneIsAlreadyLoadingMessage);
                return OperationResult.Failure(sceneIsAlreadyLoadingMessage);
            }
            endOfFrameTask = new UniTaskCompletionSource<OperationResult>();
            currentLoadingSceneName = sceneName;
            
            SystemsController.OnEndFrame += LoadSceneAtEndFrame;
            await endOfFrameTask.Task;
            SystemsController.OnEndFrame -= LoadSceneAtEndFrame;
            endOfFrameTask = null;

            if(loadingSceneTask == null)
            {
                string errorMessage = $"Failed to load scene({sceneName}).";
                Debug.LogError(errorMessage);
                return OperationResult.Failure(errorMessage);
            }

            OperationResult result = await loadingSceneTask.Value;

            currentLoadingSceneName = NO_SCENE_LOADING;
            loadingSceneTask = null;
            return result;
            
            void LoadSceneAtEndFrame()
            {
                if(loadingSceneTask != null)
                {
                    return;
                }
                loadingSceneTask = LoadScene(sceneName);
                endOfFrameTask.TrySetResult(OperationResult.Success());
            }
        }


        // public UniTask RemoveScene(string sceneName)
        // {
        //     return UniTask.CompletedTask;
        // }
        

        #endregion


        #region Internal
        
        private async UniTask<OperationResult> LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            {
                OperationResult uninstallCurrentSceneSystems = await UninstallCurrentSceneSystems();
                if(!uninstallCurrentSceneSystems.IsSuccess)
                {
                    string sceneNames = GetCurrenSceneNamesDebugLabel();
                    Debug.LogError($"Failed to uninstall current scenes({sceneNames}) systems.\n Error: {uninstallCurrentSceneSystems.Exception}");
                    return OperationResult.Failure(uninstallCurrentSceneSystems.Exception);
                }

                await LoadSceneGameobjectsAsync(sceneName, mode);

                SceneBootstrap[] sceneInstallers = GameObject.FindObjectsByType<SceneBootstrap>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                
                foreach (SceneBootstrap sceneInstaller in sceneInstallers)
                {
                    SystemsInstallerBase installer = sceneInstaller.GetLogicInstaller();
                    
                    Bootstrapper.AddInstaller(installer);
                    Bootstrapper.AddCurrentSceneInstaller(installer);
                }

                return await Bootstrapper.Run();
            }
            
            
            static UniTask LoadSceneGameobjectsAsync(string sceneName, LoadSceneMode mode)
            {
                AsyncOperation asyncOperationHandle = SceneManager.LoadSceneAsync(sceneName, mode);
                return asyncOperationHandle.ToUniTask();
            }
        }

        private string GetCurrenSceneNamesDebugLabel()
        {
            string result = "";
            currentSceneNames.ForEach(sceneName => result += $"<{sceneName}>");
            return result;
        }

        private async UniTask<OperationResult> UninstallCurrentSceneSystems()
        {
            using CachedList<SystemsInstallerBase> currentSceneLogicInstallers = ListCache<SystemsInstallerBase>.Get();
            currentSceneLogicInstallers.AddRange(Bootstrapper.GetCurrentSceneInstallers());
            
            foreach (SystemsInstallerBase currentSceneLogicInstaller in currentSceneLogicInstallers)
            {
                currentSceneLogicInstaller.UninstallSystems();
                Bootstrapper.RemoveInstaller(currentSceneLogicInstaller);
            }
            currentSceneLogicInstallers.Clear();
            
            return OperationResult.Success();
        }

        #endregion


        public void Initialize()
        {
            Debug.Log($"SceneController initialized.");
        }
        
    }
}