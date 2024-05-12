namespace Core.Systems
{
    public abstract class BasicUpdatableSystem : IUpdateSystem
    {
        public bool Active { get; set; } = true;
        public abstract void Update();
    }
}