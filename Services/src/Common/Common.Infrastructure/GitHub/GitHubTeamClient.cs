using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Async;
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
		/// The number of attempts to make for retryable GitHub operations.
		/// </summary>
		private const int c_numAttempts = 5;

		/// <summary>
		/// The delay between attempts of retryable GitHub operations.
		/// </summary>
		private readonly TimeSpan c_delayBetweenAttempts
			= TimeSpan.FromSeconds(2);

		/// <summary>
		/// The GitHub client.
		/// </summary>
		private readonly GitHubClient _client;

		/// <summary>
		/// An operation runner.
		/// </summary>
		private readonly IOperationRunner _operationRunner;

		/// <summary>
		/// Constructor.
		/// </summary>
		public GitHubTeamClient(GitHubClient client, IOperationRunner operationRunner)
		{
			_client = client;
			_operationRunner = operationRunner;
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
		public Task InviteUserToTeamAsync(
			string organizationName, 
			Team team, 
			string userName)
		{
			return RetryGitHubOperationIfNeededAsync
			(
				async () =>
				{
					await _client.Organization.Team.AddOrEditMembership
					(
						team.Id,
						userName,
						new UpdateTeamMembership(TeamRole.Member)
					);

					await _client.Organization.Team.GetMembershipDetails
					(
						team.Id,
						userName
					);
				}
			);
		}

		/// <summary>
		/// Removes a user from a team.
		/// </summary>
		public Task RemoveUserFromTeamAsync(
			string organizationName,
			GitHubTeam team,
			string userName)
		{
			return RetryGitHubOperationIfNeededAsync
			(
				async () =>
				{
					var result = await _client.Organization.Team.RemoveMembership
					(
						team.Id,
						userName
					);

					if (!result)
					{
						throw new ApiException
						(
							$"User {userName} not removed from team {team.Name}",
							HttpStatusCode.NotFound
						);
					}

					var teamMembers = await _client.Organization.Team
						.GetAllMembers(team.Id);

					if (teamMembers.Any(u => u.Login == userName))
					{
						throw new ApiException
						(
							$"User {userName} still on team {team.Name}",
							HttpStatusCode.NotFound
						);
					}
				}
			);
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
		public Task AddRepositoryAsync(
			string organizationName, 
			string repositoryName, 
			Team team)
		{
			return RetryGitHubOperationIfNeededAsync
			(
				async () =>
				{
					var added = await _client.Organization.Team.AddRepository
					(
						team.Id,
						organizationName,
						repositoryName
					);

					if (!added)
					{
						throw new ApiException
						(
							$"Error when adding team {team.Name} to repo {repositoryName}",
							HttpStatusCode.NotFound
						);
					}

					var repoTeams = await _client.Repository.GetAllTeams
					(
						organizationName,
						repositoryName
					);

					if (!repoTeams.Any(rt => rt.Name == team.Name))
					{
						throw new NotFoundException
						(
							$"Team {team.Name} cannot access repo {repositoryName}",
							HttpStatusCode.NotFound
						);
					}
				}
			);
		}

		/// <summary>
		/// Executes a GitHub operation, retrying the operation if it 
		/// fails with an ApiException.
		/// </summary>
		private Task RetryGitHubOperationIfNeededAsync(
			Func<Task> operation)
		{
			return _operationRunner.RetryOperationIfNeededAsync
			(
				operation,
				ex => ex is ApiException,
				c_numAttempts,
				c_delayBetweenAttempts,
				defaultResultIfFailed: false
			);
		}
	}
}
