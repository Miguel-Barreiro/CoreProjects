using Core.Zenject.Source.Install.Contexts;
using UnityEngine;

namespace Core.Initialization
{
    [RequireComponent(typeof(SceneContext))]
    public abstract class SceneBootstrap : RuntimeBootstrapBase
    {
        
        public override void InstallBindings()
        {
            Bootstrapper bootstrapper = GetBootstrapper();

            SystemsInstallerBase sceneInstaller = GetLogicInstaller();
            bootstrapper.AddInstaller(sceneInstaller);

            bootstrapper.AddCurrentSceneInstaller(sceneInstaller);
            
            //TODO: we can store now the logic installer to understand what systems to destroy when the corresponding scene 
            // is destroyed
        }
        
    }
}


