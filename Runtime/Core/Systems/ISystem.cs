namespace Core.Systems
{
    public interface ISystem
    {
        public SystemGroup Group { get; }
    }

    public struct SystemGroup
    {
        public readonly string Name;

        public SystemGroup(string name)
        {
            Name = name;
        }
    }

}