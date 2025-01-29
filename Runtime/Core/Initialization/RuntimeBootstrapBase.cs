using System;
using Core.Zenject.Source.Install;
using Core.Zenject.Source.Install.Contexts;

namespace Core.Initialization
{
    
    public abstract class RuntimeBootstrapBase : MonoInstaller, IDisposable
    {
        public abstract bool InstallComplete { get; }

        protected RunnableContext RunnableContext;
        private void Awake()
        {
            RunnableContext = GetComponent<RunnableContext>();
        }

        public void Dispose()
        {
            // Clear();
        }
    }
}
