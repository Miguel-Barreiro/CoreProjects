using System;
using UnityEngine;
using XNode;

namespace Core.VSEngine.Nodes.TestNodes
{
	[Node.CreateNodeMenu(VSNodeMenuNames.TEST_MENU +"/" + VSNodeMenuNames.UNIT_TEST_MENU +"/[ASSERT] Fail", order = VSNodeMenuNames.IMPORTANT)]
	[Node.NodeTint(VSNodeMenuNames.DEBUG_NODES_TINT)]
	[Serializable]
	public sealed class AssertFailNode : BaseTestAssertNode
	{
		protected override void ASSERT()
		{
			ASSERT_TRUE(false);
		}
	}
}