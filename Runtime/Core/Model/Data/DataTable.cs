using System;
using System.Collections.Generic;
using System.Reflection;
using Core.Core.Model.Data;
using Core.Model.Data;
using Core.Systems;
using Core.Utils.Reflection;
using Zenject;

namespace Core.Model
{
	public abstract class DataTable : IInitSystem, IDisposable
	{
		[Inject] private readonly DataConfigContainer DataConfigContainer = null!;
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
			DataConfigContainer.AddDataTable(this);
		}

		public void Dispose()
		{
			DataConfigContainer.RemoveDataTable(this);
		}
	}
}