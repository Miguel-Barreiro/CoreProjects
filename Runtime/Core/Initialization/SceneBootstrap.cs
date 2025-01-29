using Core.Zenject.Source.Install.Contexts;
using UnityEngine;

namespace Core.Initialization
{
    [RequireComponent(typeof(SceneContext))]
    public abstract class SceneBootstrap : RuntimeBootstrapBase
    {
        public abstract SystemsInstallerBase GetLogicInstaller();
        
        public override bool InstallComplete => RunnableContext != null && RunnableContext.Initialized;
        
        public void Install()
        {
            if (!InstallComplete)
            {
                RunnableContext.Run();
            }
        }
    }
}


