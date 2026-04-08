using Core.Model;
using Core.Utils;
using FixedPointy;
using UnityEngine;
using Zenject;

namespace Core.VSEngine.Nodes.Math
{
	[CreateNodeMenu(VSNodeMenuNames.MATH_MENU+"/Random Number [CONST]", order = VSNodeMenuNames.IMPORTANT)]
	public sealed class RandomNumberConstNode : ValueOnlyNode
	{
		[SerializeField]
		private float MinInclusive;

		[SerializeField]
		private float MaxInclusive;

		[Output(typeConstraint = TypeConstraint.Strict), SerializeField]
		private Fix Result;

		[Inject] private readonly IRandomSystem RandomSystem = null!;

		
		public override OperationResult<object> GetValue(string portName)
		{
			return OperationResult<object>.Success(RandomSystem.Range(MinInclusive, MaxInclusive));
		}

	}
}