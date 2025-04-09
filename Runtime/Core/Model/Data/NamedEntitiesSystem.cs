using Core.Model;
using Core.Model.ModelSystems;
using Core.Systems;
using System.Collections.Generic;
using Zenject;

namespace Core.Core.Model.Data
{

	public interface NamedEntity : IComponent { }

	public interface NamedEntitiesSystem
	{
		public void RegisterEntity(string name, EntId entityId);
		public EntId GetEntityId(string name);
	}

	public sealed class NamedEntitiesSystemModel : BaseEntity
	{
		internal Dictionary<string, EntId> NamedEntitiesByName = new ();
		internal Dictionary<EntId, string> NamedEntitiesById = new ();
	}

	public sealed class NamedEntitiesSystemImplementation : ComponentSystem<NamedEntity>, NamedEntitiesSystem
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

		public override void OnNew(NamedEntity newComponent) { }

		public override void OnDestroy(NamedEntity namedEntity)
		{
			if(NamedEntitiesSystemModel.NamedEntitiesById.TryGetValue(namedEntity.ID, out string name))
			{
				NamedEntitiesSystemModel.NamedEntitiesByName.Remove(name);
				NamedEntitiesSystemModel.NamedEntitiesById.Remove(namedEntity.ID);
			}
		}

		
		public override void Update(NamedEntity component, float deltaTime) { }
		public override SystemGroup Group { get; } = CoreSystemGroups.CoreSystemGroup;
	}
}