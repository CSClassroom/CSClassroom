namespace CSC.Common.Infrastructure.GitHub
{
	/// <summary>
	/// Validates GitHub Webhook payloads.
	/// </summary>
	public interface IGitHubWebhookValidator
	{
		/// <summary>
		/// Verifies that a GitHub webhook payload is correctly signed.
		/// </summary>
		bool VerifyWebhookPayloadSigned(byte[] content, string signature);
	}
}
