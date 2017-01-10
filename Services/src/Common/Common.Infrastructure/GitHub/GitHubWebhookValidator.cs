using System;
using System.Security.Cryptography;
using System.Text;

namespace CSC.Common.Infrastructure.GitHub
{
	/// <summary>
	/// Validates GitHub Webhook payloads.
	/// </summary>
	public class GitHubWebhookValidator : IGitHubWebhookValidator
	{
		/// <summary>
		/// The GitHub webhook secret.
		/// </summary>
		private readonly GitHubWebhookSecret _webhookSecret;

		/// <summary>
		/// Constructor.
		/// </summary>
		public GitHubWebhookValidator(GitHubWebhookSecret webhookSecret)
		{
			_webhookSecret = webhookSecret;
		}

		/// <summary>
		/// Verifies that a GitHub webhook payload is correctly signed.
		/// </summary>
		public bool VerifyWebhookPayloadSigned(byte[] content, string signature)
		{
			string secret = _webhookSecret.Value;

			using (var hmac = new HMACSHA1(Encoding.ASCII.GetBytes(secret)))
			{
				hmac.Initialize();
				var hash = hmac.ComputeHash(content);
				var str = BitConverter.ToString(hash);
				var expectedSignature = $"sha1={str.Replace("-", "").ToLower()}";

				return signature == expectedSignature;
			}
		}
	}
}
