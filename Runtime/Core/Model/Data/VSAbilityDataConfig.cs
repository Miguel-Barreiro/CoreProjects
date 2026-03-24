using System.Collections.Generic;
using Core.Model.Stats;
using Core.VSEngine;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Model.Data
{
	public interface IVSAbilityDataConfig
	{
		public ActionGraph ActionGraph { get; }
		public List<StatOverride> StatsOverride { get; }
	}

	[Icon("Assets/Core/Editor/Utils/LogicIcon.png")]
	public class VSAbilityDataConfig : DataConfig
	{
		[SerializeField] private List<ActionGraph> _actionGraph;
		public List<ActionGraph> ActionGraphs => _actionGraph;

		[Header("WORK IN PROGRESS")]
		[SerializeField] private List<StatOverride> _statsOverride;
		public List<StatOverride> StatsOverride => _statsOverride;

	}
}