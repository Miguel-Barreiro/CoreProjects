using Core.Zenject.Source.Install;
using Core.Zenject.Source.Install.Contexts;

namespace Core.Initialization
{
    
    public abstract class RuntimeBootstrapBase : MonoInstaller
    {
        public abstract SystemsInstallerBase GetLogicInstaller();
        
        protected RunnableContext RunnableContext;
        private void Awake()
        {
            RunnableContext = GetComponent<RunnableContext>();
        }
        
        protected Bootstrapper GetBootstrapper() { return Container.Resolve<Bootstrapper>(); }

        private void OnDestroy()
        {
            GetBootstrapper().RemoveInstaller(GetLogicInstaller());
        }

    }
}
