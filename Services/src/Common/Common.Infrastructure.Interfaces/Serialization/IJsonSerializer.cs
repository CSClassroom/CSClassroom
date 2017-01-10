using Newtonsoft.Json.Linq;

namespace CSC.Common.Infrastructure.Serialization
{
	/// <summary>
	/// Serializes and deserializes objects using Json.
	/// </summary>
	public interface IJsonSerializer
	{
		/// <summary>
		/// Serializes a Json object.
		/// </summary>
		string Serialize<TObject>(TObject obj);

		/// <summary>
		/// Serializes an object to a JObject.
		/// </summary>
		JObject SerializeToJObject<TObject>(TObject obj);

		/// <summary>
		/// Deserializes a Json object.
		/// </summary>
		TObject Deserialize<TObject>(string str);
	}
}
