namespace Core.Model.ModelSystems
{
	public interface IComponentData
	{
		public EntId ID { get; set; }

		public void Init();
	}

	public struct MockComponentData : IComponentData
	{
		public EntId ID { get; set; }
		public void Init() { }
	}

	public interface Component<T> : IEntity  where T : IComponentData { }

}