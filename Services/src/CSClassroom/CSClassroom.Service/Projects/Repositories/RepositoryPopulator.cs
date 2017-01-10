using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.Async;
using CSC.Common.Infrastructure.GitHub;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;
using CSC.CSClassroom.Model.Users;
using Microsoft.Extensions.Logging;

namespace CSC.CSClassroom.Service.Projects.Repositories
{
	/// <summary>
	/// Creates student project repositories.
	/// </summary>
	public class RepositoryPopulator : IRepositoryPopulator
	{
		/// <summary>
		/// The logger.
		/// </summary>
		private readonly ILogger _logger;

		/// <summary>
		/// The repository metadata retriever.
		/// </summary>
		private readonly IRepositoryMetadataRetriever _repoMetadataRetriever;

		/// <summary>
		/// The team client.
		/// </summary>
		private readonly IGitHubTeamClient _teamClient;

		/// <summary>
		/// The repository client.
		/// </summary>
		private readonly IGitHubRepositoryClient _repoClient;

		/// <summary>
		/// The operation runner.
		/// </summary>
		private readonly IOperationRunner _operationRunner;

		/// <summary>
		/// The starter commit message.
		/// </summary>
		public const string c_starterCommitMessage = "Populating starter project";

		/// <summary>
		/// The number of initial commits made during population..
		/// </summary>
		private const int c_numInitialCommits = 2;

		/// <summary>
		/// Constructor.
		/// </summary>
		public RepositoryPopulator(
			ILogger<RepositoryPopulator> logger,
			IRepositoryMetadataRetriever repoMetadataRetriever,
			IGitHubTeamClient teamClient,
			IGitHubRepositoryClient repoClient,
			IOperationRunner operationRunner)
		{
			_logger = logger;
			_teamClient = teamClient;
			_repoClient = repoClient;
			_operationRunner = operationRunner;
			_repoMetadataRetriever = repoMetadataRetriever;
		}

		/// <summary>
		/// Retunrs a list of files in a project repository.
		/// </summary>
		public async Task<IList<ProjectRepositoryFile>> GetRepoFileListAsync(
			Project project)
		{
			using
			(
				var repoFiles = await _repoClient.GetRepositoryContentsAsync
				(
					project.Classroom.GitHubOrganization,
					project.TemplateRepoName,
					null /*branchName*/,
					ArchiveStore.Memory
				)
			)
			{
				return repoFiles.Files.Select
				(
					entry => new ProjectRepositoryFile
					(
						project.GetFileType(entry),
						entry.FullPath
					)
				).ToList();
			}		
		}

		/// <summary>
		/// Creates repositories for the given students, and pushes the non-private
		/// files from the source project to the new repository.
		/// </summary>
		public async Task<IList<CreateStudentRepoResult>> CreateReposAsync(
			Project project,
			IList<ClassroomMembership> students,
			string webhookUrl,
			bool overwriteIfSafe)
		{
			var orgName = project.Classroom.GitHubOrganization;
			var templateRepoName = project.TemplateRepoName;
			var teams = await _teamClient.GetAllTeamsAsync(orgName);
			var repositories = await _repoClient.GetAllRepositoriesAsync(orgName);
			using
			(
				var templateContents = await _repoClient.GetRepositoryContentsAsync
				(
					orgName,
					templateRepoName,
					null /*branchName*/,
					ArchiveStore.Memory
				)
			)
			{
				return await _operationRunner.RunOperationsAsync
				(
					students,
					async student => new CreateStudentRepoResult
					(
						student.User,
						await CreateAndPushAsync
						(
							project,
							student,
							webhookUrl,
							overwriteIfSafe,
							teams,
							repositories,
							templateContents
						)
					)
				);
			}
		}

		/// <summary>
		/// Ensures that webhooks are present in all student repositories.
		/// </summary>
		public async Task EnsureWebHooksPresentAsync(
			Project project,
			IList<ClassroomMembership> students,
			string webhookUrl)
		{
			var studentRepos = await _repoMetadataRetriever.GetStudentRepositoriesAsync
			(
				project, 
				students
			);

			await _operationRunner.RunOperationsAsync
			(
				studentRepos.Values,
				repo => EnsureWebHookPresentAsync(repo, webhookUrl)
			);
		}

		/// <summary>
		/// Ensures that the webhook is present for the repository.
		/// </summary>
		private async Task<bool> EnsureWebHookPresentAsync(
			GitHubRepository repo,
			string webhookUrl)
		{
			try
			{
				await _repoClient.EnsurePushWebhookAsync(repo, webhookUrl);

				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(0, ex, "Exception ensuring that webhook is present for {orgName}/{repoName}",
					repo.Owner,
					repo.Name);

				return false;
			}
		}

		/// <summary>
		/// Creates a repository for the given student, and pushes the non-test files
		/// from the source project to the new repository.
		/// </summary>
		private async Task<CreateAndPushResult> CreateAndPushAsync(
			Project project,
			ClassroomMembership student,
			string webhookUrl,
			bool overwriteIfSafe,
			ICollection<GitHubTeam> teams,
			ICollection<GitHubRepository> repositories,
			IArchive templateContents)
		{
			string orgName = project.Classroom.GitHubOrganization;
			string repoName = $"{project.Name}_{student.GitHubTeam}";

			try
			{
				var repository = repositories.SingleOrDefault(repo => repo.Name == repoName);
				var team = teams.First(teamCandidate => teamCandidate.Name == student.GitHubTeam);
				bool repositoryAlreadyExisted = (repository != null);

				if (repositoryAlreadyExisted)
				{
					if (!overwriteIfSafe)
						return CreateAndPushResult.Exists;

					var commits = await _repoClient.GetAllCommitsAsync(orgName, repoName);
					if (commits.Count > c_numInitialCommits)
						return CreateAndPushResult.Exists;
				}
				else
				{
					repository = await _repoClient.CreateRepositoryAsync
					(
						orgName,
						repoName,
						team,
						overwrite: false
					);

					var staffTeam = GetStaffTeam(project.Classroom, teams);
					if (staffTeam != null)
					{
						await _teamClient.AddRepositoryAsync(orgName, repoName, staffTeam);
					}
				}

				await _repoClient.OverwriteRepositoryAsync
				(
					repository,
					c_starterCommitMessage,
					templateContents,
					entry => project.GetFileType(entry) != FileType.Private,
					entry => project.GetFileType(entry) == FileType.Immutable
				);

				await _repoClient.EnsurePushWebhookAsync(repository, webhookUrl);

				return repositoryAlreadyExisted
					? CreateAndPushResult.Overwritten
					: CreateAndPushResult.Created;
			}
			catch (Exception ex)
			{
				_logger.LogError
				(
					(EventId)0,
					ex,
					"Failed to create repository {RepoName} in organization {Org}.", repoName, orgName
				);

				return CreateAndPushResult.Failed;
			}
		}

		/// <summary>
		/// Returns the staff team, if any.
		/// </summary>
		private GitHubTeam GetStaffTeam(Classroom classroom, ICollection<GitHubTeam> allTeams)
		{
			return allTeams.FirstOrDefault(team => team.Name == $"{classroom.Name}_Staff");
		}
	}
}
