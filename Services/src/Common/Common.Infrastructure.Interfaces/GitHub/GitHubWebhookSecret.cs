namespace CSC.Common.Infrastructure.GitHub
{
	/// <summary>
	/// The webhook secret to use to validate GitHub webhook pushes. 
	/// (This is a separate type to enable dependency injection.)
	/// </summary>
	public class GitHubWebhookSecret
	{
		/// <summary>
		/// The GitHub webhook secret
		/// </summary>
		public string Value { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public GitHubWebhookSecret(string secret)
		{
			Value = secret;
		}
	}
}
