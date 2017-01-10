using System;
using System.Collections.Generic;

namespace CSC.Common.Infrastructure.GitHub
{
	/// <summary>
	/// A GitHub commit.
	/// </summary>
	public class GitHubCommit
	{
		/// <summary>
		/// The SHA.
		/// </summary>
		public string Sha { get; }

		/// <summary>
		/// The organization name.
		/// </summary>
		public string OrgName { get; }

		/// <summary>
		/// The repository name.
		/// </summary>
		public string RepoName { get; }

		/// <summary>
		/// The user name.
		/// </summary>
		public string User { get; }

		/// <summary>
		/// The message of the commit.
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// The date the commit was made.
		/// </summary>
		public DateTimeOffset Date { get; }

		/// <summary>
		/// The SHAs of parent commits.
		/// </summary>
		public IList<string> Parents { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public GitHubCommit(
			string sha, 
			string orgName, 
			string repoName, 
			string user,
			string message,
			DateTimeOffset date,
			IList<string> parents)
		{
			Sha = sha;
			OrgName = orgName;
			RepoName = repoName;
			User = user;
			Message = message;
			Date = date;
			Parents = parents;
		}
	}
}
