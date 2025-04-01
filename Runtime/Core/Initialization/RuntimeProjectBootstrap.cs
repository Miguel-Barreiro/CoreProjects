using Core.Systems;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Initialization
{
    public abstract class RuntimeProjectBootstrap : RuntimeBootstrapBase
    {
        [SerializeField] private Canvas rootUIPrefab = null;
        
        public override void InstallBindings()
        {
            SetupBootstrapper();
            
            Bootstrapper bootstrapper = GetBootstrapper();

            CoreSystemsInstallerForRuntime coreSystemsInstaller = new CoreSystemsInstallerForRuntime(rootUIPrefab, transform, Container);
            bootstrapper.AddInstaller(coreSystemsInstaller,false);
            bootstrapper.AddInstaller(GetLogicInstaller(), false);

            RunFullGameSetup().Forget();

            //TODO: we can store now the logic installer to understand what systems to destroy when the corresponding scene 
            // is destroyed
        }

        private void SetupBootstrapper()
        {
            if (!Container.HasBinding<Bootstrapper>())
            {
                Bootstrapper bootstrapper = new Bootstrapper(Container);
                Container.BindInstance(bootstrapper);
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