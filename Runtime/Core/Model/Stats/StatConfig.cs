using Core.Utils;
using FixedPointy;
using UnityEngine;

namespace Core.Model.Stats
{
	[CreateAssetMenu(fileName = "NewStat", menuName = "!Game/Stats/Create Stat")]
	public sealed class StatConfig : ScriptableObject
	{
		[SerializeField] [Utils.ReadOnly] 
		private string Id = UUID.Generate();
		
		[SerializeField] private string name = "NO_NAME";
		public string Name => name;
		
		[SerializeField] private float defaultBaseValue = 0;
		public Fix DefaultBaseValue => defaultBaseValue;

		[SerializeField] private int defaultMaxValue = Fix.MaxInteger;
		public Fix DefaultMaxValue => defaultMaxValue;

		[SerializeField] private int defaultMinValue = 0;
		public Fix DefaultMinValue => defaultMinValue;


		
		
		
	}
}