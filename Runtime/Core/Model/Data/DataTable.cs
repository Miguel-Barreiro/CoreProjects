using System;
using System.Collections.Generic;
using System.Reflection;
using Core.Core.Model.Data;
using Core.Systems;
using Core.Utils.Reflection;
using UnityEngine;
using Zenject;

namespace Core.Model.Data
{
	public abstract class DataTable : ScriptableObject, IInitSystem, IDisposable
	{
		[Inject] private readonly DataConfigContainer DataConfigContainer = null!;
		public IEnumerable<(DataConfig, string name)> GetDataConfigs()
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
					yield return (value as DataConfig, field.Name);
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