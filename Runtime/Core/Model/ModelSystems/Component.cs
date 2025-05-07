namespace Core.Model.ModelSystems
{
	public interface IComponentData
	{
		public EntId ID { get; set; }
	}

	public struct MockComponentData : IComponentData
	{
		public EntId ID { get; set; }
	}

	public interface Component<T> where T : IComponentData
	{
		public EntId ID { get; set; }
	}

}