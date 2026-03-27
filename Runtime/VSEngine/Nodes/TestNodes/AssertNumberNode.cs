using System;
using Core.Utils;
using FixedPointy;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

namespace Core.VSEngine.Nodes.TestNodes
{
	[Node.CreateNodeMenu(VSNodeMenuNames.TEST_MENU +"/" + VSNodeMenuNames.UNIT_TEST_MENU +"/[ASSERT] Number Equal", order = VSNodeMenuNames.IMPORTANT)]
	[Node.NodeTint(VSNodeMenuNames.DEBUG_NODES_TINT)]
	[Serializable]
	public class AssertNumberNode : BaseTestAssertNode
	{
		[SerializeField, HideLabel] private float ValueFloat;
		
		[Input(typeConstraint = TypeConstraint.Strict, 
				connectionType = ConnectionType.Override, 
				backingValue = ShowBackingValue.Never), SerializeField]
		private Fix Input;


		protected override void Assert()
		{
			OperationResult<Fix> operationResult = Resolve<Fix>(nameof(Input));
			// if(Check(operationResult.IsFailure, "Failed to resolve input"))
			// 	return false;
			ASSERT_EXIST(operationResult);
			ASSERT_EQUAL((Fix)ValueFloat , operationResult.Result);
		}
	}
}