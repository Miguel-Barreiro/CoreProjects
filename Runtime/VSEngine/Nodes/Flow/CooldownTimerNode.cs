using Core.Model;
using Core.Model.Stats;
using Core.Utils;
using Core.VSEngine.Systems;
using FixedPointy;
using UnityEngine;
using VSEngine;
using Zenject;

#nullable enable

namespace Core.VSEngine.Nodes
{
	public enum VSTimerRunningBehaviour
	{
		Ignore,   // entering while the timer is running goes through OnCooldown and keeps the timer as is
		Restart,  // entering while the timer is running restarts it with the current duration
	}

	/// <summary>
	/// Starts a timer for the graph owner. Continue executes right away, Finished executes when the timer reaches 0.
	/// The timer is updated by <see cref="VSTimersSystem"/>.
	/// </summary>
	[CreateNodeMenu(VSNodeMenuNames.FLOW_MENU+"/Cooldown Timer", order = VSNodeMenuNames.IMPORTANT)]
	[NodeTint(VSNodeMenuNames.FLOW_NODES_TINT)]
	[NodeWidth(220)]
	public sealed class CooldownTimerNode : ExecutableNode, IValueNode
	{
		[Inject] private readonly VSTimersSystem VSTimersSystem = null!;

		// duration in seconds
		// [Input(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Override,
		// 		backingValue = ShowBackingValue.Never), SerializeField]
		// private Fix Duration;

		
		// [Output(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Override, backingValue = ShowBackingValue.Never), SerializeField]
		// private Control? OnCooldown;

		[Output(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Override, backingValue = ShowBackingValue.Never), SerializeField]
		private Control? OnTrigger;

		[Output(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Multiple), SerializeField]
		private Fix TimeLeft;
		
		
		[SerializeField] private StatConfig _TimerStat;
		public StatConfig TimerStat => _TimerStat;
		
		
		// public ExecutableNode? FinishedNode => GetNodeFromPort(nameof(OnTrigger)) as ExecutableNode;

		public override void Execute()
		{
			// EntId owner = ExecutionControl.OwnerId;
		
			// if (VSTimersSystem.IsRunning(owner, this))
			// {
			// 	ContinueWith(nameof(OnCooldown));
			// 	return;
			// }
		
			// OperationResult<Fix> duration = Resolve<Fix>(nameof(Duration));
			// if (Check(duration.IsFailure, "Failed to resolve duration input"))
			// 	return;
			//
			// VSTimersSystem.StartTimer(owner, this, (float)duration.Result);
			ContinueWith(nameof(OnTrigger));
		}

		public OperationResult<object> GetValue(string portName)
		{
			EntId owner = ExecutionControl.OwnerId;
			return portName switch
			{
				nameof(TimeLeft) => SUCCESS_RETURN<object>((Fix)VSTimersSystem.GetTimeLeft(owner, _TimerStat)),
				// nameof(IsRunning) => SUCCESS_RETURN<object>(VSTimersSystem.IsRunning(owner, this)),
				_ => INVALID_INPUT_NAME_CALLED<object>(portName),
			};
		}
		
	}
}
