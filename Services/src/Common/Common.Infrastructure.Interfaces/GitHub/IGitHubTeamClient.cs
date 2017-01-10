using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSC.Common.Infrastructure.GitHub
{
	/// <summary>
	/// Performs team-related operations.
	/// </summary>
	public interface IGitHubTeamClient
	{
		/// <summary>
		/// Returns all teams in the GitHub organization.
		/// </summary>
		Task<ICollection<GitHubTeam>> GetAllTeamsAsync(string organizationName);

		/// <summary>
		/// Returns all teams in the GitHub organization.
		/// </summary>
		Task<GitHubTeam> GetTeamAsync(string organizationName, string teamName);

		/// <summary>
		/// Creates a new team.
		/// </summary>
		Task<GitHubTeam> CreateTeamAsync(string organizationName, string teamName);

		/// <summary>
		/// Adds a repository to the given team.
		/// </summary>
		Task AddRepositoryAsync(string organizationName, string repositoryName, GitHubTeam team);

		/// <summary>
		/// Invites a user to a team.
		/// </summary>
		Task InviteUserToTeamAsync(string organizationName, GitHubTeam team, string userName);

		/// <summary>
		/// Removes a user from a team.
		/// </summary>
		Task RemoveUserFromTeamAsync(string organizationName, GitHubTeam team, string userName);
	}
}
