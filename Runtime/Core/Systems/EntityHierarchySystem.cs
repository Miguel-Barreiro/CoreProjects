using System.Collections.Generic;
using Core.Model;
using Core.Model.ModelSystems;
using Zenject;

#nullable enable

namespace Core.Systems
{
	public interface IEntityHierarchySystem
	{
        public void AddChild(EntId parentId, EntId childID);
        public void RemoveChild(EntId parentId, EntId childId);
		
		public EntId? GetParent(EntId childID);
		public IEnumerable<EntId> GetChildren(EntId parentID); 
		
	}
	
	public sealed class EntityHierarchySystemImplementation : IEntityHierarchySystem, 
															IOnDestroyEntitySystem
	{
		[Inject] private readonly EntityHierarchyModel EntityHierarchyModel = null!;

		public void AddChild(EntId parentId, EntId childID)
		{
			if (EntityHierarchyModel.ParentsByChildIDs.TryGetValue(childID, out EntId currentParentID))
			{
				if(currentParentID == parentId && currentParentID != EntId.Invalid)
					return; // Child already has this parent, no need to update
			}

			EntityHierarchyModel.ParentsByChildIDs[childID] = parentId;
			
			if (!EntityHierarchyModel.ChildrenByParentIDs.TryGetValue(parentId, out var children))
			{
				children = new List<EntId>();
				EntityHierarchyModel.ChildrenByParentIDs[parentId] = children;
			}
			children.Add(childID);
		}

		public void RemoveChild(EntId parentId, EntId childId)
		{
			if (EntityHierarchyModel.ChildrenByParentIDs.TryGetValue(parentId, out var children))
				children.Remove(childId);

			EntityHierarchyModel.ParentsByChildIDs.Remove(childId);
		}

		public EntId? GetParent(EntId childID)
		{
			return EntityHierarchyModel.ParentsByChildIDs.TryGetValue(childID, out var parent) ? parent : null;
		}

		public IEnumerable<EntId> GetChildren(EntId parentID)
		{
			if (EntityHierarchyModel.ChildrenByParentIDs.TryGetValue(parentID, out var children))
			{
				foreach (EntId childID in children)
					yield return childID;
			}
		}

		public void OnDestroyEntity(EntId destroyedEntityID)
		{
			{
				if(EntityHierarchyModel.ChildrenByParentIDs.TryGetValue(destroyedEntityID, out var children))
				{
					foreach (EntId childID in children)
						DestroyChildRecursive(childID);
				}
				EntityHierarchyModel.ChildrenByParentIDs.Remove(destroyedEntityID);
				EntityHierarchyModel.ChildrenByParentIDs.Remove(destroyedEntityID);
			}
			
			void DestroyChildRecursive(EntId childID)
			{
				if(EntityHierarchyModel.ChildrenByParentIDs.TryGetValue(childID, out var children))
					foreach (EntId grandChild in children)
						DestroyChildRecursive(grandChild);
				
				EntityHierarchyModel.ChildrenByParentIDs.Remove(childID);
				EntityHierarchyModel.ParentsByChildIDs.Remove(childID);
				EntitiesContainer.DestroyEntity(childID);
			}
		}
	}
	
}