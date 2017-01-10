using System.Threading.Tasks;
using Octokit;

namespace CSC.Common.Infrastructure.GitHub
{
	/// <summary>
	/// Performs organization-related operations.
	/// </summary>
	public class GitHubOrganizationClient : IGitHubOrganizationClient
	{
		/// <summary>
		/// The GitHub client.
		/// </summary>
		private GitHubClient _client;

		/// <summary>
		/// Constructor.
		/// </summary>
		public GitHubOrganizationClient(GitHubClient client)
		{
			_client = client;
		}

		/// <summary>
		/// Checks if a given member belongs to a given organization.
		/// </summary>
		public async Task<bool> CheckMemberAsync(string organizationName, string userName)
		{
			return await _client.Organization.Member.CheckMember(organizationName, userName);
		}
	}
}
