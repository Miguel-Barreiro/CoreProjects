using Core.Utils;
using UnityEngine;

namespace Core.Model.Data
{
	public abstract class DataConfig : ScriptableObject
	{
		[SerializeField] [Utils.ReadOnly] 
		private string Id = UUID.Generate();
		public string ID => Id;

		[SerializeField] private string name = "NO_NAME";
		public string Name => name;

	}
}