using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CSC.Common.Infrastructure.Serialization
{
	/// <summary>
	/// Serializes and deserializes objects using json.
	/// </summary>
	public class ModelSerializer : IJsonSerializer
	{
		/// <summary>
		/// The json settings provider.
		/// </summary>
		private readonly IJsonSettingsProvider _jsonSettingsProvider;

		/// <summary>
		/// The settings object to use.
		/// </summary>
		private readonly JsonSerializerSettings _jsonSerializerSettings;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="jsonSettingsProvider">The json settings provider.</param>
		/// <param name="typeMaps">A list of type maps, keyed by base type.
		/// Each entry in a typemap consists of a string representation of 
		/// a sub type, along with the actual subclass type.</param>
		public ModelSerializer(IJsonSettingsProvider jsonSettingsProvider)
		{
			_jsonSettingsProvider = jsonSettingsProvider;
			_jsonSerializerSettings = new JsonSerializerSettings();
			_jsonSettingsProvider.PopulateSettings(_jsonSerializerSettings);
		}

		/// <summary>
		/// Serializes a Json object.
		/// </summary>
		public string Serialize<TObject>(TObject obj)
		{
			return Serialize(obj, writeTypesForAllSubclasses: false);
		}

		/// <summary>
		/// Serializes a Json object.
		/// </summary>
		public string Serialize<TObject>(TObject obj, bool writeTypesForAllSubclasses)
		{
			if (writeTypesForAllSubclasses)
			{
				var settings = new JsonSerializerSettings();
				_jsonSettingsProvider.PopulateSettings(settings);
				settings.TypeNameHandling = TypeNameHandling.Auto;

				return JsonConvert.SerializeObject(obj, settings);
			}
			else
			{
				return JsonConvert.SerializeObject(obj, _jsonSerializerSettings);
			}
		}

		/// <summary>
		/// Serializes an object to a JObject.
		/// </summary>
		public JObject SerializeToJObject<TObject>(TObject obj)
		{
			var serializer = new JsonSerializer();
			_jsonSettingsProvider.PopulateSettings(serializer);

			return JObject.FromObject(obj, serializer);
		}

		/// <summary>
		/// Deserializes a Json object.
		/// </summary>
		public TObject Deserialize<TObject>(string str)
		{
			return JsonConvert.DeserializeObject<TObject>(str, _jsonSerializerSettings);
		}
	}
}
