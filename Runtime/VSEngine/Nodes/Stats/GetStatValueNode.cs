using Core.Model;
using Core.Model.Stats;
using Core.Utils;
using FixedPointy;
using UnityEngine;
using XNode;
using Zenject;

namespace Core.VSEngine.Nodes.Stats
{
	
	[CreateNodeMenu(VSNodeMenuNames.STATS_MENU+"/Get Stat Value", order = VSNodeMenuNames.IMPORTANT)]
	public sealed class GetStatValueNode : ValueOnlyNode
	{
		[Inject] private readonly StatsSystem StatsSystem = null!;
		
		[Input(ShowBackingValue.Unconnected, ConnectionType.Override), SerializeField]
		private EntId Owner;

		[SerializeField] private StatConfig Stat;
		[SerializeField] private bool Depleted = false;
		
		public override OperationResult<object> GetValue(string portName)
		{
			Fix statValue = Depleted ? StatsSystem.GetStatDepletedValue(Owner, Stat) : StatsSystem.GetStatValue(Owner, Stat);
			return SUCCESS_RETURN<object>(statValue);
		}
	}
}