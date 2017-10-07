using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;

namespace CSC.Common.Infrastructure.GitHub
{
	using Team = GitHubTeam;

	/// <summary>
	/// Performs team-related operations.
	/// </summary>
	public class GitHubTeamClient : IGitHubTeamClient
	{
		/// <summary>
		/// The GitHub client.
		/// </summary>
		private readonly GitHubClient _client;

		/// <summary>
		/// Constructor.
		/// </summary>
		public GitHubTeamClient(GitHubClient client)
		{
			_client = client;
		}

		/// <summary>
		/// Returns all teams.
		/// </summary>
		public async Task<ICollection<Team>> GetAllTeamsAsync(
			string organizationName)
		{
			var allTeams = await _client.Organization.Team.GetAll(organizationName);

			return allTeams
				.Select(team => new Team(team.Id, team.Name))
				.ToList();
		}

		/// <summary>
		/// Creates a new team.
		/// </summary>
		public async Task<Team> CreateTeamAsync(
			string organizationName, 
			string teamName)
		{
			var team = await GetTeamAsync(organizationName, teamName);
			if (team != null)
				return team;

			var newTeam = await _client.Organization.Team.Create
			(
				organizationName, 
				new NewTeam(teamName)
				{
					Permission = Permission.Push
				}
			);

			return new Team(newTeam.Id, newTeam.Name);
		}

		/// <summary>
		/// Invites a user to a team.
		/// </summary>
		public async Task InviteUserToTeamAsync(
			string organizationName, 
			Team team, 
			string userName)
		{
			await _client.Organization.Team.AddOrEditMembership
			(
				team.Id, 
				userName, 
				new UpdateTeamMembership(TeamRole.Member)
			);
		}


		/// <summary>
		/// Removes a user from a team.
		/// </summary>
		public async Task RemoveUserFromTeamAsync(
			string organizationName,
			GitHubTeam team,
			string userName)
		{
			await _client.Organization.Team.RemoveMembership(team.Id, userName);
		}

		/// <summary>
		/// Returns the given team.
		/// </summary>
		public async Task<Team> GetTeamAsync(
			string organizationName, 
			string teamName)
		{
			var teams = await _client.Organization.Team.GetAll(organizationName);
			var team = teams.SingleOrDefault(t => t.Name == teamName);

			if (team == null)
				return null;

			return new Team(team.Id, team.Name);
		}

		/// <summary>
		/// Adds a repository to the given team.
		/// </summary>
		public async Task AddRepositoryAsync(
			string organizationName, 
			string repositoryName, 
			Team team)
		{
			await _client.Organization.Team.AddRepository
			(
				team.Id, 
				organizationName, 
				repositoryName
			);
		}
	}
}
