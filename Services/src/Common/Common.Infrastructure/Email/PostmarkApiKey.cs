namespace CSC.Common.Infrastructure.Email
{
	/// <summary>
	/// The API key to use for the Postmark service.
	/// (This is a separate type to enable dependency injection.)
	/// </summary>
	public class PostmarkApiKey
	{
		/// <summary>
		/// The API key.
		/// </summary>
		public string ApiKey { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public PostmarkApiKey(string apiKey)
		{
			ApiKey = apiKey;
		}
	}
}
