namespace Core.Model.ModelSystems
{
	
	public interface IComponentSystem<T>  where T : IComponentData { }
		
	
	
	
	
	public interface OnCreateComponent<T> : IComponentSystem<T> 
		where T : IComponentData
	{
		void OnCreateComponent(ref T newComponent);
	}
	
	public interface OnDestroyComponent<T> : IComponentSystem<T> 
		where T : IComponentData
	{
		void OnDestroyComponent(ref T destroyedComponent);
	}
	
	public interface UpdateComponent<T> : IComponentSystem<T> 
		where T : IComponentData
	{
		void UpdateComponents(float deltaTime);
	}
	
	
}