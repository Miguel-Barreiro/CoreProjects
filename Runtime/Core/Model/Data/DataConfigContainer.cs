using System.Collections.Generic;
using Core.Model;
using Core.Model.Data;

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
			IEnumerable<DataConfig> dataTableConfigs = dataTable.GetDataConfigs();
			foreach (DataConfig dataConfig in dataTableConfigs)
			{
				dataConfigs.Add(dataConfig.ID, dataConfig);
			}
		}

		public void RemoveDataTable(DataTable dataTable)
		{
			IEnumerable<DataConfig> dataTableConfigs = dataTable.GetDataConfigs();
			foreach (DataConfig dataConfig in dataTableConfigs)
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