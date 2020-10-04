using System;
using System.Collections.Generic;

namespace CSC.Common.Infrastructure.GitHub
{
	/// <summary>
	/// A push event from a GitHub webhook.
	/// </summary>
	public class GitHubPushEvent
	{
		/// <summary>
		/// The repository for which commits were pushed.
		/// </summary>
		public PushEventRepository Repository { get; set; }

		/// <summary>
		/// The full Git ref that was pushed. Example: "refs/heads/master".
		/// </summary>
		public string Ref { get; set; }

		/// <summary>
		/// The SHA of the most recent commit on ref after the push.
		/// </summary>
		public string After { get; set; }

		/// <summary>
		/// The list of commits in the push event.
		/// </summary>
		public IList<GitHubPushEventCommit> Commits { get; set; }

		/// <summary>
		/// The date the push occured.
		/// </summary>
		public DateTimeOffset CreatedAt { get; set; }
	}

	/// <summary>
	/// A repository for a GitHub push event.
	/// </summary>
	public class PushEventRepository
	{
		/// <summary>
		/// The name of the repository.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The owner of the repository.
		/// </summary>
		public PushEventRepositoryOwner Owner { get; set; }

		/// <summary>
		/// The default branch of the repository.
		/// </summary>
		public string Default_Branch { get; set; }
	}

	/// <summary>
	/// The owner of a repository in a GitHub push event.
	/// </summary>
	public class PushEventRepositoryOwner
	{
		/// <summary>
		/// The name of the owner.
		/// </summary>
		public string Name { get; set; }
	}
}
