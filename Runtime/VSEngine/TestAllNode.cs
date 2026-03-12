using Core.Utils;
using FixedPointy;
using UnityEngine;
using XNode;

namespace Core.VSEngine
{
	[Node.CreateNodeMenu("Miguel/testing/TestAllNode")]
	[Node.NodeTint("#3d4254")]
	public class TestAllNode : BasicFlowNode {

		[SerializeField]
		[TextArea(2, 10)]
		private string Header;

		[Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict), SerializeField]
		private Fix In;
	
		[Output(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict), SerializeField]
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