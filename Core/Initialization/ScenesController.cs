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
                    if (!sceneInstaller.Complete)
                    {
                        sceneInstaller.Install();
                    }
                }
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
        }
    }
}