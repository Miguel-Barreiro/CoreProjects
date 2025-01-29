using System.Collections.Generic;
using Core.Systems;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Core.Initialization
{
    public sealed class ScenesController : IInitSystem
    {
        [Inject] private readonly SystemsController systemsController = null!;

        #region Public

        
        public UniTask AddScene(string sceneName)
        {
            return UniTask.CompletedTask;
        }

        public UniTask SwitchScene(string sceneName)
        {
            return UniTask.CompletedTask;
        }
        
        public UniTask RemoveScene(string sceneName)
        {
            return UniTask.CompletedTask;
        }
        

        #endregion


        #region Internal
        
        private async void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            {
                await LoadSceneGameobjectsAsync(sceneName, mode);

                List<SystemsInstallerBase> sceneLogicInstallers = new List<SystemsInstallerBase>();
                
                SceneBootstrap[] sceneInstallers = GameObject.FindObjectsByType<SceneBootstrap>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (SceneBootstrap sceneInstaller in sceneInstallers)
                {
                    if (!sceneInstaller.InstallComplete)
                    {
                        sceneInstaller.Install();
                    }
                    
                    sceneLogicInstallers.Add(sceneInstaller.GetLogicInstaller());
                }

                FinishSetupSystems(sceneLogicInstallers).Forget();
            }
            
            
            static UniTask LoadSceneGameobjectsAsync(string sceneName, LoadSceneMode mode)
            {
                AsyncOperation asyncOperationHandle = SceneManager.LoadSceneAsync(sceneName, mode);
                return asyncOperationHandle.ToUniTask();
            }
        }

        

        #endregion


        public void Initialize()
        {
            Debug.Log($"SceneController initialized.");

            SceneBootstrap[] sceneInstallers = GameObject.FindObjectsByType<SceneBootstrap>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            List<SystemsInstallerBase> newBaseInstallers = new List<SystemsInstallerBase>(sceneInstallers.Length);
            foreach (SceneBootstrap runtimeInstaller in sceneInstallers)
            {
                newBaseInstallers.Add(runtimeInstaller.GetLogicInstaller());
            }
            FinishSetupSystems(newBaseInstallers).Forget();
        }

        private async UniTask FinishSetupSystems(List<SystemsInstallerBase> logicInstallers)
        {
            {
                bool systemsInitialized = false;
                while (!systemsInitialized)
                {
                    await UniTask.DelayFrame(1);
                    systemsInitialized = GetAllSystemsInitialized(logicInstallers);
                }

                List<UniTask> loadingTasks = new List<UniTask>();
                foreach (SystemsInstallerBase baseInstaller in logicInstallers)
                {
                    loadingTasks.Add(baseInstaller.LoadSystems());
                }
                await UniTask.WhenAll(loadingTasks);
                
                foreach (SystemsInstallerBase baseInstaller in logicInstallers)
                {
                    baseInstaller.StartSystems();
                }
            }

            static bool GetAllSystemsInitialized( List<SystemsInstallerBase> baseInstallers)
            {
                foreach (SystemsInstallerBase baseInstaller in baseInstallers)
                {
                    if (!baseInstaller.InstallComplete)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}