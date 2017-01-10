using Microsoft.AspNetCore.Mvc;

namespace CSC.CSClassroom.WebApp.Extensions
{
	/// <summary>
	/// Extensions for an IUrlHelper
	/// </summary>
	public static class IUrlHelperExtensions
	{
		/// <summary>
		/// The GitHub URL helper.
		/// </summary>
		private static GitHubUrlHelper s_gitHubUrlHelper
			= new GitHubUrlHelper();

		/// <summary>
		/// Returns the GitHub URL helper.
		/// </summary>
		public static GitHubUrlHelper GitHub(this IUrlHelper urlHelper) => s_gitHubUrlHelper;
	}

	/// <summary>
	/// Returns GitHub urls.
	/// </summary>
	public class GitHubUrlHelper
	{
		/// <summary>
		/// Returns the URL for an invitation to a GitHub organization.
		/// </summary>
		public string Invitation(string orgName)
		{
			return GitHubUrl($"orgs/{orgName}/invitation");
		}

		/// <summary>
		/// Returns the URL for a commit in a repository.
		/// </summary>
		public string Commit(string orgName, string repoName, string sha)
		{
			return GitHubUrl($"{orgName}/{repoName}/commit/{sha}");
		}

		/// <summary>
		/// Returns the URL for a pull request.
		/// </summary>
		public string PullRequest(
			string orgName, 
			string repoName, 
			int pullRequestNumber)
		{
			return GitHubUrl($"{orgName}/{repoName}/pull/{pullRequestNumber}/files?diff=unified");
		}

		/// <summary>
		/// Creates a GitHub url
		/// </summary>
		private string GitHubUrl(string path)
		{
			return $"https://github.com/login?return_to=/{path}";
		}
	}
}
