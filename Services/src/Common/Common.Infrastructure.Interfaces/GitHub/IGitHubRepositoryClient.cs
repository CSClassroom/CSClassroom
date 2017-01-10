using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.System;

namespace CSC.Common.Infrastructure.GitHub
{
	/// <summary>
	/// Performs repository-related operations.
	/// </summary>
	public interface IGitHubRepositoryClient
	{
		/// <summary>
		/// Returns all repositories in the GitHub organization.
		/// </summary>
		Task<ICollection<GitHubRepository>> GetAllRepositoriesAsync(
			string organizationName);

		/// <summary>
		/// Returns the given repository in the GitHub organization.
		/// </summary>
		Task<GitHubRepository> GetRepositoryAsync(
			string organizationName, 
			string repositoryName);

		/// <summary>
		/// Returns the given repository in the GitHub organization.
		/// </summary>
		Task<IArchive> GetRepositoryContentsAsync(
			string organizationName,
			string repositoryName,
			string branchName,
			ArchiveStore backingStore);

		/// <summary>
		/// Creates a new repository.
		/// </summary>
		Task<GitHubRepository> CreateRepositoryAsync(
			string organizationName,
			string repositoryName,
			GitHubTeam team,
			bool overwrite);

		/// <summary>
		/// Returns all commits for the given repository.
		/// </summary>
		Task<ICollection<GitHubCommit>> GetAllCommitsAsync(
			string organizationName, 
			string repositoryName);

		/// <summary>
		/// Overwrites an existing repository with an existing archive.
		/// All existing commits will be erased, and the repository will
		/// be populated with two commits.
		/// </summary>
		Task OverwriteRepositoryAsync(
			GitHubRepository repository,
			string commitMessage,
			IArchive contents,
			Func<IArchiveFile, bool> includeFile,
			Func<IArchiveFile, bool> includeInFirstCommit);

		/// <summary>
		/// Ensures that a push webhook exists for the repository.
		/// </summary>
		Task EnsurePushWebhookAsync(
			GitHubRepository repository, 
			string webhookUrl);

		/// <summary>
		/// Returns a list of push events for the given repository.
		/// </summary>
		Task<IList<GitHubPushEvent>> GetPushEventsAsync(
			string orgName, 
			string repoName);

		/// <summary>
		/// Ensures that a branch is created with the given commit.
		/// If the branch already exists, its reference is updated.
		/// </summary>
		Task CreateBranchAsync(
			string orgName,
			string repoName,
			string branchName,
			string commitSha);

		/// <summary>
		/// Ensures that a branch is created with the given commit.
		/// If the branch already exists, its reference is updated.
		/// </summary>
		Task DeleteBranchAsync(
			string orgName,
			string repoName,
			string branchName);

		/// <summary>
		/// Creates a pull request, and returns the pull request number.
		/// </summary>
		Task<int> CreatePullRequestAsync(
			string orgName,
			string repoName,
			string title,
			string sourceBranchName,
			string destBranchName);
	}
}
