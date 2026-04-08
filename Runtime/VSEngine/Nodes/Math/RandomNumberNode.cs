using Core.Model;
using Core.Utils;
using FixedPointy;
using UnityEngine;
using Zenject;

namespace Core.VSEngine.Nodes.Math
{

	[CreateNodeMenu(VSNodeMenuNames.MATH_MENU+"/Random Number", order = VSNodeMenuNames.IMPORTANT)]
	public class RandomNumberNode : ValueOnlyNode
    {
		[Input(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Override, 
				backingValue = ShowBackingValue.Never), SerializeField]
        private Fix MinInclusive;

		[Input(typeConstraint = TypeConstraint.Strict, connectionType = ConnectionType.Override, 
				backingValue = ShowBackingValue.Never), SerializeField]
        private Fix MaxInclusive;

        [Output(typeConstraint = TypeConstraint.Strict), SerializeField]
        private Fix Result;

		[Inject] private readonly IRandomSystem RandomSystem = null!;

		
		public override OperationResult<object> GetValue(string portName)
		{
			OperationResult<Fix> minRes = Resolve<Fix>(nameof(MinInclusive));
			if (minRes.IsFailure)
				return INVALID_INPUT<object>(nameof(MinInclusive));
			OperationResult<Fix> maxRes = Resolve<Fix>(nameof(MaxInclusive));
			if (minRes.IsFailure)
				return INVALID_INPUT<object>(nameof(MaxInclusive));

			return OperationResult<object>.Success(RandomSystem.Range(MinInclusive, MaxInclusive));
		}

	}
}