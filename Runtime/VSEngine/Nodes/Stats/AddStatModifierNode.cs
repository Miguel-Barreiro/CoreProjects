using Core.Model;
using Core.Model.Stats;
using Core.Utils;
using FixedPointy;
using UnityEngine;
using UnityEngine.Serialization;
using VSEngine;
using XNode;
using Zenject;

namespace Core.VSEngine.Nodes.Stats
{
	
	[Node.CreateNodeMenu(VSNodeMenuNames.STATS_MENU+"/Add Modifier", order = VSNodeMenuNames.IMPORTANT)]
	[Node.NodeTint(VSNodeMenuNames.FLOW_NODES_TINT)]
	[Node.NodeWidth(220)]
	public sealed class AddStatModifierNode : BasicFlowNode
	{
		[Inject] private readonly StatsSystem StatsSystem = null!;

		
		[Node.Input(ShowBackingValue.Unconnected, Node.ConnectionType.Override), SerializeField] 
		private StatConfig Stat;

		[Node.Input(ShowBackingValue.Never, Node.ConnectionType.Override), SerializeField] 
		private Fix Value;
		
		[Node.Input(ShowBackingValue.Never, Node.ConnectionType.Override), SerializeField] 
		private EntId Owner;
		
		[Node.Input(ShowBackingValue.Never, Node.ConnectionType.Override), SerializeField] 
		private EntId Target;

		[SerializeField] private StatModifierType OverrideType = StatModifierType.Additive;
		
		
		protected override void Action()
		{
			
			OperationResult<StatConfig> statResult = Resolve<StatConfig>(nameof(Stat));
			if (statResult.IsFailure)
			{
				Debug.LogError($"Failed to resolve Stat for AddStatModifierNode {name} in graph {graph.name}: {statResult}");
				return;
			}
			OperationResult<Fix> valueResult = Resolve<Fix>(nameof(Value));
			if (valueResult.IsFailure)
			{
				Debug.LogError($"Failed to resolve NewValue for AddStatModifierNode {name} in graph {graph.name}: {valueResult}");
				return;
			}

			OperationResult<EntId> ownerResult = Resolve<EntId>(nameof(Owner));
			if (ownerResult.IsFailure)
			{
				Debug.LogError($"Failed to resolve owner for AddStatModifierNode {name} in graph {graph.name}: {ownerResult}");
				return;
			}

			OperationResult<EntId> targetResult = Resolve<EntId>(nameof(Target));
			if (targetResult.IsFailure)
			{
				Debug.LogError($"Failed to resolve target for AddStatModifierNode {name} in graph {graph.name}: {targetResult}");
				return;
			}
			
			StatsSystem.AddModifier(ownerResult.Result, targetResult.Result, 
									statResult.Result, valueResult.Result, OverrideType);
		}
		public override OperationResult<object> GetValue(string portName)
		{
			return OperationResult<object>.Failure("AddStatModifierNode does not return a value.");
		}
	}
}