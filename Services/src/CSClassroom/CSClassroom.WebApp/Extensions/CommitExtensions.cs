using CSC.CSClassroom.Model.Projects;
using Microsoft.AspNetCore.Mvc;

namespace CSC.CSClassroom.WebApp.Extensions
{
	/// <summary>
	/// Extensions for a commit.
	/// </summary>
	public static class CommitExtensions
	{
		/// <summary>
		/// Returns a GitHub url.
		/// </summary>
		public static string GetCommitUrl(this Commit commit, IUrlHelper urlHelper)
		{
			var gitHubOrg = commit
				.Project
				.Classroom
				.GitHubOrganization;

			var repoName = commit.GetRepoName();

			var commitSha = commit.Sha;

			return urlHelper.GitHub().Commit(gitHubOrg, repoName, commitSha);
		}
	}
}
