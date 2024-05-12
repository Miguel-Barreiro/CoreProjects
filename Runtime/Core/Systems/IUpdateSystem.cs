namespace Core.Systems
{
    public interface IUpdateSystem
    {
        bool Active { get; set; }
        void Update();
    }
}