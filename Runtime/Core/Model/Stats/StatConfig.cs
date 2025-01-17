using FixedPointy;
using UnityEngine;

namespace Core.Model
{
	[CreateAssetMenu(fileName = "NewStat", menuName = "Core/Stats/Create Stat")]
	public sealed class StatConfig : ScriptableObject
	{
		[SerializeField] private Fix defaultBaseValue = 0;
		public Fix DefaultBaseValue => defaultBaseValue;


		[SerializeField] private Fix defaultMaxValue = Fix.MaxValue;
		public Fix DefaultMaxValue => defaultMaxValue;

		[SerializeField] private Fix defaultMinValue = 0;
		public Fix DefaultMinValue => defaultMinValue;
		
	}
}