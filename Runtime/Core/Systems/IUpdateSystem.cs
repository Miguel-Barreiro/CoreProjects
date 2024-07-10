namespace Core.Systems
{
    public interface IUpdateSystem : ISystem
    {
        void UpdateSystem(float deltaTime);
        
    }
}