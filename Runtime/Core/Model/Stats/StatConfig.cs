using FixedPointy;
using UnityEngine;

namespace Core.Model
{
	[CreateAssetMenu(fileName = "NewStat", menuName = "Core/Stats/Create Stat")]
	public sealed class StatConfig : ScriptableObject
	{
		[SerializeField] private float defaultBaseValue = 0;
		public Fix DefaultBaseValue => defaultBaseValue;

		[SerializeField] private int defaultMaxValue = int.MaxValue;
		public Fix DefaultMaxValue => defaultMaxValue;

		[SerializeField] private int defaultMinValue = 0;
		public Fix DefaultMinValue => defaultMinValue;
		
	}
}