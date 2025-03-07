using Core.Model.Data;
using FixedPointy;
using UnityEngine;

namespace Core.Model.Stats
{
	[CreateAssetMenu(fileName = "NewStat", menuName = "!Game/Stats/Create Stat")]
	public sealed class StatConfig : DataConfig
	{
		[SerializeField] private float defaultBaseValue = 0;
		public Fix DefaultBaseValue => defaultBaseValue;

		[SerializeField] private int defaultMaxValue = Fix.MaxInteger;
		public Fix DefaultMaxValue => defaultMaxValue;

		[SerializeField] private int defaultMinValue = 0;
		public Fix DefaultMinValue => defaultMinValue;
		
	}
}