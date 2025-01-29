namespace Core.Initialization
{
    public abstract class RuntimeProjectBootstrap : RuntimeBootstrapBase
    {
        public override bool InstallComplete => installComplete;
        private bool installComplete = false;

        protected abstract SystemsInstallerBase GetLogicInstaller();
        
        public override void InstallBindings()
        {
            InstantiateInternalSystems();
            GetLogicInstaller().InstallSystems(Container);
            
            //TODO: we can store now the logic installer to understand what systems to destroy when the corresponding scene 
            // is destroyed
        }

        protected void InstantiateInternalSystems()
        {
            CoreSystemsInstaller coreSystemsInstaller = new CoreSystemsInstaller(transform);
            coreSystemsInstaller.InstallSystems(Container);
        }

    }
}