using System;
using Core.Model.Stats;
using Game.Simulation.Map.Importing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core.Core.Runtime.Core.Model.Stats
{
	
	/// <summary>
	///
	///	  JsonConvert.SerializeObject(objectToSerialize, GetJsonSerializerSettings(tables));
	/// 
	///   private static JsonSerializerSettings GetJsonSerializerSettings (Tables tables) => new() {
	///			TypeNameHandling = TypeNameHandling.All,
	///				Converters = new List<JsonConverter> {
	///				new StatConfigConverter(StatTable),    
	///			}
	///		};
	/// 
	/// </summary>
	public sealed class StatConfigConverter : JsonConverter
	{
		
		private readonly IStatTable StatTable;
		public StatConfigConverter(IStatTable statTable) { StatTable = statTable; }

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			string id = ((StatConfig)value!).Name;
			JToken token = JToken.FromObject(id);
			token.WriteTo(writer);

		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			 
			JToken token = JToken.Load(reader);

			string id = token.Value<string>()!;

			return StatTable.GetStatById(id);
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(StatConfig).IsAssignableFrom(objectType);
		}
	}
}