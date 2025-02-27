namespace Core.Systems
{
    public interface ISystem
    {
        public bool Active { get; set; }
        public SystemGroup Group { get; }

        static SystemGroup DefaultGroup { get; } = new SystemGroup("Default");
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