namespace CSC.Common.Service.Serialization
{
	/// <summary>
	/// Serializes and deserializes objects using Json.
	/// </summary>
    public interface IJsonConverter
    {
		/// <summary>
		/// Serializes a Json object.
		/// </summary>
		string Serialize<TObject>(TObject obj);

		/// <summary>
		/// Deserializes a Json object.
		/// </summary>
		TObject Deserialize<TObject>(string str);
    }
}
