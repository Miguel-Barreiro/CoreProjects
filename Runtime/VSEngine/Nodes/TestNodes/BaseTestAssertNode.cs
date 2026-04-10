using Core.Utils;
using FixedPointy;
using NUnit.Framework;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

namespace Core.VSEngine.Nodes.TestNodes
{
	// [Node.CreateNodeMenu(VSNodeMenuNames.TEST_MENU+"/" + VSNodeMenuNames.UNIT_TEST_MENU +"/Assert", order = VSNodeMenuNames.LOW)]
	[Node.NodeTint(VSNodeMenuNames.DEBUG_NODES_TINT)]
	public abstract class BaseTestAssertNode : BasicFlowNode {

		[SerializeField, Space(10)]
		private int RepeatNumber = 1;

		// [SerializeField]
		// [TextArea(2, 10)]
		// private string Header;

		
		// [Input(ShowBackingValue.Unconnected, ConnectionType.Override, TypeConstraint.Strict), SerializeField]
		// private Fix In;
		//
		// [Output(ShowBackingValue.Unconnected, ConnectionType.Override, TypeConstraint.Strict), SerializeField]
		// private Fix Out;
		
		public override OperationResult<object> GetValue(string portName)
		{
			return OperationResult<object>.Failure($"{name} [{GetType().Name}] in {graph.name} should not have any value output ports");
		}


		protected override void Action()
		{
			// Debug.Log($"{Header} in node {this.name} in {graph.name}");
			for (int i = 0; i < RepeatNumber; i++)
				ASSERT();
		}

		protected abstract void ASSERT();


		protected void ASSERT_TRUE(bool value)
		{
				Assert.That(value , expression:Is.True, 
							message: $"\nIn <{name}> [ {value} != TRUE ] in graph {graph.name}");
		}

		
		protected void ASSERT_OUT_LIMIT(Fix value, Fix minExpected, Fix maxExpected)
		{
			Assert.That(value > maxExpected, expression:Is.True, 
						message: $"In <{name}> value is within limit [ value({value}) < maxLimitExpected({maxExpected} ] in graph {graph.name}");
			Assert.That(value < minExpected, expression:Is.True, 
						message: $"In <{name}> value is within limit [ value({value}) > minExpected({minExpected} ] in graph {graph.name}");

		}

		protected void ASSERT_LIMIT(Fix value, Fix minExpected, Fix maxExpected)
		{
			Assert.That(value <= maxExpected, expression:Is.True, 
						message: $"In <{name}> value is outside limit [ value({value}) > maxExpected({maxExpected} ] in graph {graph.name}");
			Assert.That(value >= minExpected, expression:Is.True, 
						message: $"In <{name}> value is outside limit [ value({value}) < minExpected({minExpected} ] in graph {graph.name}");

		}

		
		protected void ASSERT_EQUAL<T>(T expected, T actual)
		{
			Assert.That(expected, expression:Is.EqualTo(actual), 
						message: $"In <{name}> value should be equal [ expected({expected}) != actual({actual}) ] in graph {graph.name}");
		}

		protected void ASSERT_DIFERENT<T>(T expected, T actual)
		{
			Assert.That(expected, expression:Is.Not.EqualTo(actual), 
						message: $"In <{name}> value should be different [ expected({expected}) == actual({actual} ]  in graph {graph.name}");
		}

		
		protected void ASSERT_EXIST<T>(OperationResult<T> input, string inputName)
		{
			if (input.IsFailure)
			{
				Assert.That(input.IsSuccess, 
							Is.True, 
							message: $"node {name}({GetType().Name}) Failed to resolve input {inputName} in graph {graph.name}: \n {input.Exception.Message}");
			} else
			{
				Assert.That(input.IsSuccess, Is.True);
			}
		}
		
		// protected override void Action()
		// {
		// 	OperationResult<Fix> operationResult = Resolve<Fix>(nameof(In));
		// 	if (operationResult.IsSuccess)
		// 	{
		// 		Debug.Log($"<TEST_ALL_NODE><{operationResult.Result}>");
		// 	}
		//
		// 	Debug.Log("<TEST_ALL_NODE>" + Header);
		// 	
		// 	Assert.That(children.Count, Is.EqualTo(1));
		// }

	}
}