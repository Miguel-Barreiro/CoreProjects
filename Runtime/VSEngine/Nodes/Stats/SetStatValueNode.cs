using Core.Model;
using Core.Model.Stats;
using Core.Utils;
using FixedPointy;
using UnityEngine;
using Zenject;

namespace Core.VSEngine.Nodes.Stats
{
	[CreateNodeMenu(VSNodeMenuNames.STATS_MENU+"/Set Stat Value", order = VSNodeMenuNames.IMPORTANT)]
	public sealed class SetStatValueNode : BaseStatNode
	{
		
		[SerializeField] private bool SetDepletedValue = false;
		
		[Input(ShowBackingValue.Never, ConnectionType.Override), SerializeField]
		private Fix NewValue;
		
		protected override void Action()
		{
			if(SetDepletedValue)
				StatsSystem.SetDepletedValue(Owner, Stat, NewValue);
			else
				StatsSystem.SetBaseValue(Owner, Stat, NewValue);			
		}
		
		public override OperationResult<object> GetValue(string portName)
		{
			return	INVALID_EXECUTION<object>($"SetStatValueNode {name} in graph {graph.name} does not have any output ports.");
		}
	}

	[NodeTint(VSNodeMenuNames.STAT_NODES_TINT)]
	public abstract class BaseStatNode : BasicFlowNode 
	{
		[Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict), SerializeField]
		protected EntId Owner;
		
		[Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict), SerializeField]
		protected StatConfig Stat;
		
		[Inject] protected readonly StatsSystem StatsSystem = null!;
	}
}