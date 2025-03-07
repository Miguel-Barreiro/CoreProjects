using System;
using Core.Core.Model.Data;
using Core.Model.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core.Core.Runtime.Core.Model.Stats
{
	
	/// <summary>
	///
	///	  JsonConvert.SerializeObject(objectToSerialize, GetJsonSerializerSettings(tables));
	/// 
	///   private static JsonSerializerSettings GetJsonSerializerSettings (DataConfigContainer dataConfigContainer) => new() {
	///			TypeNameHandling = TypeNameHandling.All,
	///				Converters = new List<JsonConverter> {
	///				new StatConfigConverter(StatTable),    
	///			}
	///		};
	/// 
	/// </summary>
	public sealed class DataConfigConverter : JsonConverter
	{
		private readonly IDataConfigContainer DataConfigContainer;
		public DataConfigConverter(DataConfigContainer dataConfigContainer) { DataConfigContainer = dataConfigContainer; }

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			string id = ((DataConfig)value!).Name;
			JToken token = JToken.FromObject(id);
			token.WriteTo(writer);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JToken token = JToken.Load(reader);
			string id = token.Value<string>()!;
			return DataConfigContainer.GetDataConfig(id);
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(DataConfig).IsAssignableFrom(objectType);
		}
	}
}