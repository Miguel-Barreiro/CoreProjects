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
		void UpdateComponents(float deltaTime);
	}

	
	public interface IOnDestroyEntitySystem
	{
		void OnDestroyEntity(EntId destroyedEntityId);
	}

	public interface IOnCreateEntitySystem
	{
		void OnCreateEntity(EntId destroyedEntityId);
	}

	
	public interface IOnUninstallSystem
	{
		void OnUninstall(object system);
	}
	
}