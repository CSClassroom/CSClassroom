using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CSC.Common.Service.Serialization
{
	/// <summary>
	/// Provides JSON settings used for all services.
	/// </summary>
	public class JsonSettingsProvider : IJsonSettingsProvider
    {
		/// <summary>
		/// Populates json settings used for serialization. This property should
		/// only be used directly for serialization automatically performed
		/// by the framework, which requires an JsonSerializerSettings object.
		/// All other serialization should be performed using a json converter.
		/// <see cref="IJsonConverter"/>
		/// </summary>
		/// <param name="settings">The settings object to populate.</param>
		public void PopulateSettings(JsonSerializerSettings settings)
		{
			settings.ContractResolver = new ApiContractResolver();
			settings.NullValueHandling = NullValueHandling.Ignore;
			settings.Formatting = Formatting.Indented;
		}
    }
}
