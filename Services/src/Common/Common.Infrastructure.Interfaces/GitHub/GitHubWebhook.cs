namespace CSC.Common.Infrastructure.GitHub
{
	/// <summary>
	/// A GitHub webhook.
	/// </summary>
	public class GitHubWebhook
	{
		/// <summary>
		/// The name of the webhook.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The URL of the webhook.
		/// </summary>
		public string Url { get; }

		/// <summary>
		/// The secret for the webhook.
		/// </summary>
		public string Secret { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public GitHubWebhook(string name, string url, string secret)
		{
			Name = name;
			Url = url;
			Secret = secret;
		}
	}
}
