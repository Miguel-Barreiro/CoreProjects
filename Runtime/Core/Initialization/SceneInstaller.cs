using Zenject;

namespace Core.Initialization
{
    
    public abstract class SceneInstaller : BaseInstaller
    {
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


