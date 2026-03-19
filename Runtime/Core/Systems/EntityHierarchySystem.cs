using System.Collections.Generic;
using System.Runtime.InteropServices;
using Core.Model;
using Core.Model.ModelSystems;
using Core.Utils;
using UnityEngine;
using Zenject;

#nullable enable

namespace Core.Systems
{
	public interface IEntityHierarchySystem
	{
        public void AddChild(EntId parentId, EntId childID);
        public void RemoveChild(EntId parentId, EntId childId);
		
		public EntId GetParent(EntId childID);
		public List<EntId> GetChildrenList(EntId parentID);

	}



	[StructLayout(LayoutKind.Auto)]
	public struct HierarchyData : IComponentData
	{
		public EntId ID { get; set; }
		public EntId ParentID { get; set; }
		
		public List<EntId> ChildsID { get; private set; }

		public void Init()
		{
			this.ChildsID.Clear();
			this.ParentID = EntId.Invalid;
		}

		public void Reset()
		{
			ChildsID = new List<EntId>();
			ID = EntId.Invalid;
			ParentID = EntId.Invalid;
		}
	}

	public interface IHierarchyEntity : Component<HierarchyData> { }
	
	
	public sealed class EntityHierarchySystemImplementation : IEntityHierarchySystem, 
															OnDestroyComponent<HierarchyData>
	{

		[Inject] private readonly BasicCompContainer<HierarchyData> HierarchyContainer = null!;

		public void AddChild(EntId parentId, EntId childID)
		{
			ref HierarchyData childHierarchyData = ref HierarchyContainer.GetComponent(childID);
			ref HierarchyData parentHierarchyData = ref HierarchyContainer.GetComponent(parentId);

			if (childHierarchyData.ID == EntId.Invalid)
			{
				Debug.LogError($"Trying to add child {childID.Id} to parent {parentId.Id}, " +
								$"but child does not exist or does not have HierarchyData component.");
				return;
			}

			if (parentHierarchyData.ID == EntId.Invalid)
			{
				Debug.LogError($"Trying to add child {childID.Id} to parent {parentId.Id}," +
								$" but parent does not exist or does not have HierarchyData component.");
				return;
			}

			
			if (childHierarchyData.ParentID != EntId.Invalid)
			{
				ref HierarchyData previousParentdHierarchyData = ref HierarchyContainer.GetComponent(childHierarchyData.ParentID);
				previousParentdHierarchyData.ChildsID.Remove(childID);
			}


			if(parentHierarchyData.ChildsID.Contains(childID))
				return;

			parentHierarchyData.ChildsID.Add(childID);
			childHierarchyData.ParentID = parentId;
			
		}

		public void RemoveChild(EntId parentId, EntId childID)
		{
			ref HierarchyData childHierarchyData = ref HierarchyContainer.GetComponent(childID);
			ref HierarchyData parentHierarchyData = ref HierarchyContainer.GetComponent(parentId);

			if (childHierarchyData.ID == EntId.Invalid)
			{
				Debug.LogError($"Trying to add child {childID.Id} to parent {parentId.Id}, " +
								$"but child does not exist or does not have HierarchyData component.");
				return;
			}

			if (parentHierarchyData.ID == EntId.Invalid)
			{
				Debug.LogError($"Trying to add child {childID.Id} to parent {parentId.Id}," +
								$" but parent does not exist or does not have HierarchyData component.");
				return;
			}
			
			
			childHierarchyData.ParentID = EntId.Invalid;
			parentHierarchyData.ChildsID.Remove(childID);
		}

		public EntId GetParent(EntId childID)
		{
			ref HierarchyData childHierarchyData = ref HierarchyContainer.GetComponent(childID);
			return childHierarchyData.ParentID;
		}
		
		public List<EntId> GetChildrenList(EntId parentID)
		{
			ref HierarchyData parentHierarchyData = ref HierarchyContainer.GetComponent(parentID);
			return parentHierarchyData.ChildsID;
		}

		public void OnDestroyComponent(EntId destroyedComponentId)
		{
			ref HierarchyData destroyedHierarchyData = ref HierarchyContainer.GetComponent(destroyedComponentId);
			
			EntId parentId = destroyedHierarchyData.ParentID;
			if (parentId != EntId.Invalid)
				RemoveChild(parentId, destroyedComponentId);

			foreach (EntId childId in destroyedHierarchyData.ChildsID)
			{
				ref HierarchyData childHierarchyData = ref HierarchyContainer.GetComponent(childId);
				childHierarchyData.ParentID = EntId.Invalid;
				EntitiesContainer.DestroyEntity(childId);
			}
		}

		public bool Active { get; set; } = true;
		public SystemGroup Group { get; } = CoreSystemGroups.CoreSystemGroup;
	}
	
}