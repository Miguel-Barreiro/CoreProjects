using System;
using Core.Model;
using Core.Model.Stats;
using Core.Utils;
using FixedPointy;
using NUnit.Framework;
using UnityEngine;
using XNode;
using Zenject;

namespace Core.VSEngine.Nodes.TestNodes
{
	[Node.CreateNodeMenu(VSNodeMenuNames.TEST_MENU +"/" + VSNodeMenuNames.UNIT_TEST_MENU +"/[ASSERT] Stat Equal", order = VSNodeMenuNames.IMPORTANT)]
	[Node.NodeTint(VSNodeMenuNames.DEBUG_NODES_TINT)]
	[Serializable]
	public class AssertStatNode : BaseTestAssertNode
	{
		[SerializeField] private StatConfig Stat;
		[SerializeField] private float Value;
		[SerializeField] private float DepletedValue;
		
		[Inject] private readonly StatsSystem StatsSystem = null!;
		
		[Input(typeConstraint = TypeConstraint.Strict, 
				connectionType = ConnectionType.Override, 
				backingValue = ShowBackingValue.Never), SerializeField]
		private EntId Entity;


		protected override void Assert()
		{
			OperationResult<EntId> operationResult = Resolve<EntId>(nameof(Input));
			ASSERT_EXIST(operationResult);

			// NUnit.Framework.Assert.That(operationResult.IsFailure, Is.Not.True);
			// if(Check(operationResult.IsFailure, $"Failed to resolve {nameof(Entity)} port"))
			// 	return;

			EntId entId = operationResult.Result;
			Fix statValue = StatsSystem.GetStatValue(entId, Stat);

			ASSERT_EQUAL((Fix)Value, statValue);
			
			// NUnit.Framework.Assert.That((Fix)Value, Is.EqualTo(statValue));
			Fix depletedValue = StatsSystem.GetStatDepletedValue(entId, Stat);

			ASSERT_EQUAL((Fix)DepletedValue, depletedValue);
			// NUnit.Framework.Assert.That((Fix)DepletedValue, Is.EqualTo(depletedValue));
		}
	}

}