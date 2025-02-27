using System.Collections.Generic;
using Core.Systems;
using Core.Utils;
using Core.Utils.CachedDataStructures;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Core.Initialization
{
    public interface ScenesController
    {
        // public UniTask AddScene(string sceneName);
        public UniTask SwitchScene(string sceneName);
        // public UniTask RemoveScene(string sceneName);
    }


    public sealed class ScenesControllerImplemention : IInitSystem, ScenesController
    {
        [Inject] private readonly SystemsController systemsController = null!;
        [Inject] private readonly Bootstrapper Bootstrapper = null!;

        
        private List<string> currentSceneNames = new List<string>();
        
        #region Public

        //
        // public UniTask AddScene(string sceneName)
        // {
        //     return UniTask.CompletedTask;
        // }

        public UniTask SwitchScene(string sceneName)
        {
            LoadScene(sceneName);
            
            return UniTask.CompletedTask;
        }
        
        // public UniTask RemoveScene(string sceneName)
        // {
        //     return UniTask.CompletedTask;
        // }
        

        #endregion


        #region Internal
        
        private async void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            {
                OperationResult uninstallCurrentSceneSystems = await UninstallCurrentSceneSystems();
                if(!uninstallCurrentSceneSystems.IsSuccess)
                {
                    string sceneNames = GetCurrenSceneNamesDebugLabel();
                    Debug.LogError($"Failed to uninstall current scenes({sceneNames}) systems.\n Error: {uninstallCurrentSceneSystems.Exception}");
                    return;
                }

                await LoadSceneGameobjectsAsync(sceneName, mode);

                SceneBootstrap[] sceneInstallers = GameObject.FindObjectsByType<SceneBootstrap>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                
                foreach (SceneBootstrap sceneInstaller in sceneInstallers)
                {
                    SystemsInstallerBase installer = sceneInstaller.GetLogicInstaller();
                    
                    Bootstrapper.AddInstaller(installer);
                    Bootstrapper.AddCurrentSceneInstaller(installer);
                }

                bool setupResult = await Bootstrapper.Run();
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