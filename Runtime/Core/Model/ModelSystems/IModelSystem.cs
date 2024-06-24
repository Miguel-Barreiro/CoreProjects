using Core.Systems;

namespace Core.Model.ModelSystems
{
    public interface IModelSystem : ISystem
    {
        public bool Active { get; }
    }

}