using Core.Systems;

namespace Core.Model.ModelSystems
{
	
	public interface IComponentSystem<T> : ISystem
		where T : struct, IComponentData { }
	
	
	public interface OnCreateComponent<T> : IComponentSystem<T> 
		where T : struct, IComponentData
	{
		void OnCreateComponent(EntId newComponentId);
	}
	
	
	public interface OnDestroyComponent<T> : IComponentSystem<T> 
		where T : struct, IComponentData
	{
		void OnDestroyComponent(EntId destroyedComponentId);
	}
	
	public interface UpdateComponents<T> : IComponentSystem<T>
		where T : struct, IComponentData
	{
		void UpdateComponents(ComponentContainer<T> componentsContainer, float deltaTime);
	}
	
	
}