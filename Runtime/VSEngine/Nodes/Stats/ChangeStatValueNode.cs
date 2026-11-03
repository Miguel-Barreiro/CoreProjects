using Core.Utils;
using FixedPointy;
using UnityEngine;

namespace Core.VSEngine.Nodes.Stats
{
	[CreateNodeMenu(VSNodeMenuNames.STATS_MENU+"/Change Stat Value", order = VSNodeMenuNames.IMPORTANT)]
	public class ChangeStatValueNode : BaseStatNode
	{
		[SerializeField] private bool SetDepletedValue = false;
		
		[Input(ShowBackingValue.Never, ConnectionType.Override), SerializeField]
		private Fix DeltaValue;

		
		[Output(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Override, backingValue = ShowBackingValue.Never), SerializeField]
		private Fix NewValue;
		
		protected override void Action()
		{
			OperationResult<Fix> delta = Resolve<Fix>(nameof(DeltaValue));
			if(delta.IsFailure)
			{
				Debug.LogError($"Failed to resolve DeltaValue for ChangeStatValueNode {name} in graph {graph.name}: {delta.Exception}");
				return;
			}

			if(SetDepletedValue)
				StatsSystem.ChangeDepletedValue(Owner, Stat, delta.Result);
			else
			{
				Fix statValue = StatsSystem.GetStatValue(Owner, Stat);
				StatsSystem.SetBaseValue(Owner, Stat, statValue + delta.Result);
			}

		}
		public override OperationResult<object> GetValue(string portName)
		{
			if (SetDepletedValue)
			{
				return SUCCESS_RETURN<object>(StatsSystem.GetStatDepletedValue(Owner, Stat));
			} else
			{
				return SUCCESS_RETURN<object>(StatsSystem.GetStatValue(Owner, Stat));
			}
		}
	}
}