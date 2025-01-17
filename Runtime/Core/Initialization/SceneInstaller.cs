using Core.Zenject.Source.Install.Contexts;
using UnityEngine;

namespace Core.Initialization
{
    [RequireComponent(typeof(SceneContext))]
    public abstract class SceneInstaller : BaseInstaller
    {
        public override bool InstallComplete => RunnableContext != null && RunnableContext.Initialized;

        public void Install()
        {
            RunnableContext runnableContext = GetComponent<RunnableContext>();
            if (!InstallComplete)
            {
                runnableContext.Run();
            }
        }
    }
}


