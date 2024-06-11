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

                SceneInstaller[] sceneInstallers = GameObject.FindObjectsByType<SceneInstaller>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (SceneInstaller sceneInstaller in sceneInstallers)
                {
                    if (!sceneInstaller.InstallComplete)
                    {
                        sceneInstaller.Install();
                    }
                }

                FinishSetupSystems(sceneInstallers).Forget();
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

            BaseInstaller[] baseInstallers = GameObject.FindObjectsByType<BaseInstaller>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            FinishSetupSystems(baseInstallers).Forget();
        }

        private async UniTask FinishSetupSystems(BaseInstaller[] systemInstallers)
        {
            {
                bool systemsInitialized = false;
                while (!systemsInitialized)
                {
                    await UniTask.DelayFrame(1);
                    systemsInitialized = GetAllSystemsInitialized(systemInstallers);
                }

                List<UniTask> loadingTasks = new List<UniTask>();
                foreach (BaseInstaller baseInstaller in systemInstallers)
                {
                    loadingTasks.Add(baseInstaller.LoadSystems());
                }
                await UniTask.WhenAll(loadingTasks);
                
                foreach (BaseInstaller baseInstaller in systemInstallers)
                {
                    baseInstaller.StartSystems();
                }
            }

            static bool GetAllSystemsInitialized( BaseInstaller[] baseInstallers)
            {
                foreach (BaseInstaller baseInstaller in baseInstallers)
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