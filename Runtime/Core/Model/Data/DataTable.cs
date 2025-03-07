using System;
using System.Collections.Generic;
using System.Reflection;
using Core.Model.Data;
using Core.Systems;
using Core.Utils.Reflection;

namespace Core.Model
{
	public abstract class DataTable : IInitSystem
	{
		public IEnumerable<DataConfig> GetDataConfigs()
		{
			Type selfType = this.GetType();
			FieldInfo[] fields = selfType.GetFields(BindingFlags.Public |
														BindingFlags.NonPublic |
														BindingFlags.Instance);
			foreach (FieldInfo field in fields)
			{
				if (field.FieldType.IsTypeOf<DataConfig>())
				{
					object value = field.GetValue(this);
					yield return value as DataConfig;
				}
			}
		}

		public void Initialize()
		{
			
		}
	}
}