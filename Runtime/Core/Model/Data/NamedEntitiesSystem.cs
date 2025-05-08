using Core.Model;
using Core.Model.ModelSystems;
using Core.Systems;
using System.Collections.Generic;
using Zenject;

namespace Core.Core.Model.Data
{

	public struct NamedEntityData : IComponentData
	{
		public string Name;
		public EntId ID { get; set; }

		public void Init()
		{
			Name = "";
		}
	}
		
	public interface NamedEntity : Component<NamedEntityData> { }

	public interface NamedEntitiesSystem : ISystem
	{
		public void RegisterEntity(string name, EntId entityId);
		public EntId GetEntityId(string name);
	}

	public sealed class NamedEntitiesSystemModel : Entity
	{
		internal Dictionary<string, EntId> NamedEntitiesByName = new ();
		internal Dictionary<EntId, string> NamedEntitiesById = new ();
	}

	public sealed class NamedEntitiesSystemImplementation : OnDestroyComponent<NamedEntityData>, NamedEntitiesSystem
	{

		[Inject] private readonly NamedEntitiesSystemModel NamedEntitiesSystemModel = null!;
		
		public void RegisterEntity(string name, EntId entityId)
		{
			NamedEntitiesSystemModel.NamedEntitiesByName[name] = entityId;
			NamedEntitiesSystemModel.NamedEntitiesById[entityId] = name;
		}

		public EntId GetEntityId(string name)
		{
			return NamedEntitiesSystemModel.NamedEntitiesByName.TryGetValue(name, out EntId entityId) ? entityId : EntId.Invalid;
		}


		public void OnDestroyComponent(EntId destroyedComponentId)
		{ 
			if(NamedEntitiesSystemModel.NamedEntitiesById.TryGetValue(destroyedComponentId, out string name))
			{
				NamedEntitiesSystemModel.NamedEntitiesByName.Remove(name);
				NamedEntitiesSystemModel.NamedEntitiesById.Remove(destroyedComponentId);
			}
		}

		public bool Active { get; set; } = true;
		public SystemGroup Group { get; } = CoreSystemGroups.CoreSystemGroup;
	}
}