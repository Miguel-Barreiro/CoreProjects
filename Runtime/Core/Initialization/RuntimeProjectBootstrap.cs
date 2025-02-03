using Core.Systems;
using Cysharp.Threading.Tasks;

namespace Core.Initialization
{
    public abstract class RuntimeProjectBootstrap : RuntimeBootstrapBase
    {
        
        public override void InstallBindings()
        {
            SetupBootstrapper();
            SetupSystemsContainer();
            SetupObjectBuilder();
            
            Bootstrapper bootstrapper = GetBootstrapper();

            CoreSystemsInstaller coreSystemsInstaller = new CoreSystemsInstaller(transform, Container);
            bootstrapper.AddInstaller(coreSystemsInstaller);
            bootstrapper.AddInstaller(GetLogicInstaller());

            RunFullGameSetup().Forget();

            //TODO: we can store now the logic installer to understand what systems to destroy when the corresponding scene 
            // is destroyed
        }

        
        private void SetupObjectBuilder()
        {
            ObjectBuilder objectBuilder;
		            
            if (!Container.HasBinding<ObjectBuilder>())
            {
                objectBuilder = new ObjectBuilder();
				
                Container.BindInstance(objectBuilder);
            }
            else
            {
                objectBuilder = Container.Resolve<ObjectBuilder>();
            }
			
            Container.Inject(objectBuilder);
        }

        
        private void SetupBootstrapper()
        {
            if (!Container.HasBinding<Bootstrapper>())
            {
                Bootstrapper bootstrapper = new Bootstrapper();
                Container.BindInstance(bootstrapper);
            }
        }
        
        private void SetupSystemsContainer()
        {
            if (!Container.HasBinding<SystemsContainer>())
            {
                SystemsContainer systemsContainer = new SystemsContainer();
                Container.BindInstance(systemsContainer);
            }
        }

        

        private async UniTask RunFullGameSetup()
        {
            await UniTask.DelayFrame(2);
            Bootstrapper bootstrapper = GetBootstrapper();
            await bootstrapper.Run();
        }

    }
}