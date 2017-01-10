using Newtonsoft.Json;

namespace CSC.Common.Infrastructure.Serialization
{
	/// <summary>
	/// Provides JSON settings used for all services.
	/// </summary>
	public interface IJsonSettingsProvider
	{
		/// <summary>
		/// Populates json settings used for serialization. This property should
		/// only be used directly for serialization automatically performed
		/// by the MVC framework, which requires an JsonSerializerSettings object.
		/// All other serialization should be performed using a json serializer.
		/// <see cref="IJsonSerializer"/>
		/// </summary>
		/// <param name="settings">The settings object to populate.</param>
		void PopulateSettings(JsonSerializerSettings settings);

		/// <summary>
		/// Populates json settings used for serialization. This property should
		/// only be used by implementations of IJsonSerializer.
		/// <see cref="IJsonSerializer"/>
		/// </summary>
		/// <param name="serializer">The serializer containing settings to populate.</param>
		void PopulateSettings(JsonSerializer serializer);
	}
}
