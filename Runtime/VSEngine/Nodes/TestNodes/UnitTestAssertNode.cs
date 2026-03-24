using Core.Utils;
using FixedPointy;
using UnityEngine;
using XNode;

namespace Core.VSEngine.Nodes.TestNodes
{
	[Node.CreateNodeMenu(MenuNames.TEST_MENU+"/" + MenuNames.UNIT_TEST_MENU +"/Assert", order = 2)]
	[Node.NodeTint("#3d4254")]
	public class UnitTestAssertNode : BasicFlowNode {

		[SerializeField]
		[TextArea(2, 10)]
		private string Header;

		[Input(ShowBackingValue.Unconnected, ConnectionType.Override, TypeConstraint.Strict), SerializeField]
		private Fix In;
	
		[Output(ShowBackingValue.Unconnected, ConnectionType.Override, TypeConstraint.Strict), SerializeField]
		private Fix Out;
	
		// Use this for initialization
		protected override void Init() {
			base.Init();
		
		}
		protected override void Action()
		{
			OperationResult<Fix> operationResult = Resolve<Fix>(nameof(In));
			if (operationResult.IsSuccess)
			{
				Debug.Log($"<TEST_ALL_NODE><{operationResult.Result}>");
			}

			Debug.Log("<TEST_ALL_NODE>" + Header);
		}

	}
}