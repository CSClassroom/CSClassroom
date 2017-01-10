using Newtonsoft.Json;
using TypeMap = System.Collections.Generic.IReadOnlyDictionary<string, System.Type>;

namespace CSC.Common.Infrastructure.Serialization
{
	/// <summary>
	/// Provides JSON settings used for all services.
	/// </summary>
	public class JsonSettingsProvider : IJsonSettingsProvider
	{
		/// <summary>
		/// Type maps for deserializing abstract types.
		/// </summary>
		private readonly ITypeMapCollection _typeMaps;

		/// <summary>
		/// Constructor.
		/// </summary>
		public JsonSettingsProvider(ITypeMapCollection typeMaps)
		{
			_typeMaps = typeMaps;
		}

		/// <summary>
		/// Populates json settings used for serialization. This property should
		/// only be used directly for serialization automatically performed
		/// by the framework, which requires an JsonSerializerSettings object.
		/// All other serialization should be performed using a json serializer.
		/// <see cref="IJsonSerializer"/>
		/// </summary>
		/// <param name="settings">The settings object to populate.</param>
		/// <param name="typeMaps">The type maps to use when serializing/deserializing abstract classes.</param>
		public void PopulateSettings(JsonSerializerSettings settings)
		{
			settings.ContractResolver = new ApiContractResolver(_typeMaps);
			settings.NullValueHandling = NullValueHandling.Ignore;
			settings.Formatting = Formatting.Indented;
		}

		/// <summary>
		/// Populates json settings used for serialization. This property should
		/// only be used by implementations of IJsonSerializer.
		/// <see cref="IJsonSerializer"/>
		/// </summary>
		/// <param name="serializer">The serializer containing settings to populate.</param>
		public void PopulateSettings(JsonSerializer serializer)
		{
			serializer.ContractResolver = new ApiContractResolver(_typeMaps);
			serializer.NullValueHandling = NullValueHandling.Ignore;
			serializer.Formatting = Formatting.Indented;
		}
	}
}
