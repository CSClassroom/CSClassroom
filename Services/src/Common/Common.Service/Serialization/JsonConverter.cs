using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CSC.Common.Service.Serialization
{
	/// <summary>
	/// Serializes and deserializes objects using json.
	/// </summary>
    public class JsonConverter : IJsonConverter
    {
		/// <summary>
		/// The json settings provider.
		/// </summary>
		private JsonSerializerSettings _jsonSerializerSettings;

		/// <summary>
		/// Constructor.
		/// </summary>
		public JsonConverter(IJsonSettingsProvider jsonSettingsProvider)
		{
			_jsonSerializerSettings = new JsonSerializerSettings();

			jsonSettingsProvider.PopulateSettings(_jsonSerializerSettings);
		}

		/// <summary>
		/// Serializes a Json object.
		/// </summary>
		public string Serialize<TObject>(TObject obj)
		{
			return JsonConvert.SerializeObject(obj, _jsonSerializerSettings);
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
