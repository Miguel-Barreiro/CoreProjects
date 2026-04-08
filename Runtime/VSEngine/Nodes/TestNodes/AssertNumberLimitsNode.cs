using System;
using Core.Utils;
using FixedPointy;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

namespace Core.VSEngine.Nodes.TestNodes
{
	[Node.CreateNodeMenu(VSNodeMenuNames.TEST_MENU +"/" + VSNodeMenuNames.UNIT_TEST_MENU +"/[ASSERT] Number Limits", 
							order = VSNodeMenuNames.IMPORTANT)]
	[Node.NodeTint(VSNodeMenuNames.DEBUG_NODES_TINT)]
	[Serializable]
	public class AssertNumberLimitsNode : BaseTestAssertNode
	{
		[SerializeField] private float MinInclusive;
		[SerializeField] private float MaxInclusive;
		
		[Input(typeConstraint = TypeConstraint.Strict, 
				connectionType = ConnectionType.Override, 
				backingValue = ShowBackingValue.Never), SerializeField]
		private Fix Input;

		[SerializeField] private bool ExpectTrue = true;

		protected override void ASSERT()
		{
			OperationResult<Fix> operationResult = Resolve<Fix>(nameof(Input));
			// if(Check(operationResult.IsFailure, "Failed to resolve input"))
			// 	return false;
			ASSERT_EXIST(operationResult, nameof(Input));
			if(ExpectTrue)
				ASSERT_LIMIT(operationResult.Result, MinInclusive, MaxInclusive);
			else
				ASSERT_OUT_LIMIT(operationResult.Result, MinInclusive, MaxInclusive);
		}
	}
}