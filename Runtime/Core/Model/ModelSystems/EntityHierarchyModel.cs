using System.Collections.Generic;
using UnityEngine;

namespace Core.Model.ModelSystems
{
	public sealed class EntityHierarchyModel : Entity
	{
		[SerializeField]
		public Dictionary<EntId, EntId> ParentsByChildIDs = new();
		[SerializeField]
		public Dictionary<EntId, List<EntId>> ChildrenByParentIDs = new();
		
	}
}
