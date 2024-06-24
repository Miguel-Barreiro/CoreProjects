namespace Core.Systems
{
    public interface IUpdateSystem : ISystem
    {
        bool Active { get; set; }
        void UpdateSystem(float deltaTime);
        
    }
}