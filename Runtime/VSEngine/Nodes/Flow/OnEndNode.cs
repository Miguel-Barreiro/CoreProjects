
using Core.Utils;
using UnityEngine;
using XNode;

namespace Core.VSEngine.Nodes
{
	[Node.CreateNodeMenu(VSNodeMenuNames.FLOW_MENU+"/On End", order = VSNodeMenuNames.IMPORTANT)]
	[Node.NodeTint(VSNodeMenuNames.LISTEN_NODES_TINT)]
	[Node.NodeWidth(220)]
	public sealed class OnEndNode : ExecutableNode
	{
		[Output(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Override, backingValue = ShowBackingValue.Never), SerializeField]
		private Control? Continue;

		public override void Execute()
		{
			ContinueWith(nameof(Continue));
		}
	}
}
