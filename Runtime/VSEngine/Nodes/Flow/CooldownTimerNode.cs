using Core.Model;
using Core.Model.Stats;
using Core.Utils;
using Core.VSEngine.Systems;
using FixedPointy;
using UnityEngine;
using Zenject;

#nullable enable

namespace Core.VSEngine.Nodes
{
	public enum VSTimerRunningBehaviour
	{
		Ignore,   // entering while the timer is running goes through OnCooldown and keeps the timer as is
		Restart,  // entering while the timer is running restarts it with the current duration
	}
	
	[CreateNodeMenu(VSNodeMenuNames.FLOW_MENU+"/Cooldown Timer", order = VSNodeMenuNames.IMPORTANT)]
	[NodeTint(VSNodeMenuNames.LISTEN_NODES_TINT)]
	[NodeWidth(220)]
	public sealed class CooldownTimerNode : ExecutableNode, IValueNode
	{
		[Inject] private readonly VSTimersSystem VSTimersSystem = null!;
		
		[Output(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Override, backingValue = ShowBackingValue.Never), SerializeField]
		private Control? OnTrigger;

		[Output(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Multiple), SerializeField]
		private Fix TimeLeft;

		[SerializeField] private bool Enable = true;
		public bool Enabled => Enable;
		
		[SerializeField] private int _TimerIndex = 0;
		public int TimerIndex => _TimerIndex;
		
		public StatConfig? GetTimerStat()
		{
			ActionGraph actionGraph = (ActionGraph)this.graph;
			if(TimerIndex < 0 || TimerIndex >= actionGraph.LocalTriggers.Count)
			{
				Debug.LogError($"CooldownTimerNode {name} in graph {graph.name} has an invalid TimerIndex {_TimerIndex}. It should be between 0 and {actionGraph.LocalTriggers.Count - 1}.");
				return null;
			}
			return actionGraph.LocalTriggers[TimerIndex].CooldownStat;
		}
		
		
		// public ExecutableNode? FinishedNode => GetNodeFromPort(nameof(OnTrigger)) as ExecutableNode;

		public override void Execute()
		{
			ContinueWith(nameof(OnTrigger));
		}

		public OperationResult<object> GetValue(string portName)
		{
			EntId owner = ExecutionControl.OwnerId;
			return portName switch
			{
				nameof(TimeLeft) => SUCCESS_RETURN<object>((Fix)VSTimersSystem.GetTimeLeft(owner, GetTimerStat())),
				_ => INVALID_INPUT_NAME_CALLED<object>(portName),
			};
		}
		
	}
}
