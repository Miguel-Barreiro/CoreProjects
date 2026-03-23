using System;
using Core.Model.Data;
using FixedPointy;
using UnityEngine;
using UnityEngine.Serialization;

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
		
		[SerializeField][TextArea] private string description = "";
		public string Description => description;
		
		[SerializeField] private bool canOverflow = false;
		public bool CanOverflow => canOverflow;
		

	}

	
	public enum OverrideType
	{
		Override = 0,
		Additive = 1,
		Multiplicative = 2
	}

	
	[Serializable]
	public class StatOverride
	{
		[SerializeField] private StatConfig _statConfig;
		public StatConfig StatConfig => _statConfig;

		[SerializeField] private float _value;
		public float Value => _value;

		[SerializeField] private OverrideType _overrideType;
		public OverrideType OverrideType => _overrideType;
	}
}