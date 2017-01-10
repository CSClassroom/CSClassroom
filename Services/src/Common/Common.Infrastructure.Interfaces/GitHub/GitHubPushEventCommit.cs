using System;

namespace CSC.Common.Infrastructure.GitHub
{
	/// <summary>
	/// A commit in a GitHub push event.
	/// </summary>
	public class GitHubPushEventCommit
	{
		/// <summary>
		/// The SHA.
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// The organization name.
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// The date the commit was made.
		/// </summary>
		public DateTimeOffset Timestamp { get; set; }
	}
}
