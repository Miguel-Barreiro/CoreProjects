using System.Collections.Generic;
using Core.Model;
using Core.Model.Data;
using UnityEngine;

namespace Core.Core.Model.Data
{
	public interface IDataConfigContainer
	{
		public void AddDataTable(DataTable dataTable);
		public void RemoveDataTable(DataTable dataTable);
		
		public DataConfig GetDataConfig(string id);
	}
	
	public sealed class DataConfigContainer : IDataConfigContainer
	{
		private Dictionary<string, DataConfig> dataConfigs = new Dictionary<string, DataConfig>();
		
		public void AddDataTable(DataTable dataTable)
		{
			IEnumerable<(DataConfig, string name)> dataTableConfigs = dataTable.GetDataConfigs();
			foreach ((DataConfig currentDataConfig, string name) in dataTableConfigs)
			{
				if (currentDataConfig == null)
				{
					Debug.LogError($"DataConfigContainer: DataConfig field({name}) is null for table {dataTable.name}"); 
					continue;
				}
				
				if (dataConfigs.ContainsKey(currentDataConfig.ID))
					continue;
				
				dataConfigs.Add(currentDataConfig.ID, currentDataConfig);
			}
		}

		public void RemoveDataTable(DataTable dataTable)
		{
			IEnumerable<(DataConfig, string name)> dataTableConfigs = dataTable.GetDataConfigs();
			foreach ((DataConfig dataConfig, string _) in dataTableConfigs)
			{
				if (dataConfigs.ContainsKey(dataConfig.ID))
					dataConfigs.Remove(dataConfig.ID);
			}
			
		}

		public DataConfig GetDataConfig(string id)
		{
			return dataConfigs.GetValueOrDefault(id);
		}
	}
}