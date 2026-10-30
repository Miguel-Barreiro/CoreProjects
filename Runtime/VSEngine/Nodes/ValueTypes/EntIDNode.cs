using Core.Model;
using Core.Systems;
using Core.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Core.VSEngine.Nodes
{
	[NodeWidth(300)]
	[CreateNodeMenu(VSNodeMenuNames.VALUES_MENU +"/EntID", order = VSNodeMenuNames.IMPORTANT)]
	[NodeTint(VSNodeMenuNames.VALUES_NODES_TINT)]
	public class EntIDNode : SimpleValueNode<EntId>
	{
		[Inject] private readonly IEntityHierarchySystem IEntityHierarchySystem = null!;
		
		[SerializeField] 
		private EntityTarget Target;

		public override OperationResult<object> GetValue(string portName)
		{
			switch (Target)
			{
				case EntityTarget.Owner:
					return OperationResult<object>.Success(ExecutionControl.OwnerId);
				case EntityTarget.Parent:
					EntId parentId = IEntityHierarchySystem.GetParent(ExecutionControl.OwnerId);
					if(parentId == EntId.Invalid)
					{
						// Debug.LogError($"EntIDNode {name} in graph {graph.name} could not find a parent for owner {ExecutionControl.OwnerId}");
						return OperationResult<object>.Failure($"EntIDNode {name} in graph {graph.name} could not find a parent for owner {ExecutionControl.OwnerId}");
					}
					return OperationResult<object>.Success(parentId);

				default:
					return OperationResult<object>.Failure($"EntIDNode {name} in graph {graph.name} has an invalid target {Target}");
			}
		}
	}
	
	
	public enum EntityTarget
	{
		[LabelText("Graph Owner")]
		Owner,
		
		[LabelText("Target Entity")]
		Parent,
		
	}

}