using FixedPointy;
using UnityEngine;

namespace Core.Model
{
	[CreateAssetMenu(fileName = "NewStat", menuName = "Core/Stats/Create Stat")]
	public sealed class StatConfig : ScriptableObject
	{
		[SerializeField] private float defaultBaseValue = 0;
		public float DefaultBaseValue => defaultBaseValue;

		[SerializeField] private int defaultMaxValue = int.MaxValue;
		public int DefaultMaxValue => defaultMaxValue;

		[SerializeField] private int defaultMinValue = 0;
		public int DefaultMinValue => defaultMinValue;
		
	}
}