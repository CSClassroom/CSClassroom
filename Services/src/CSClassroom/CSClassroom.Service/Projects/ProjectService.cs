using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.GitHub;
using CSC.Common.Infrastructure.Serialization;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Projects.PushEvents;
using CSC.CSClassroom.Service.Projects.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CSC.CSClassroom.Service.Projects
{
	/// <summary>
	/// Performs project operations.
	/// </summary>
	public class ProjectService : IProjectService
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// Creates student repositories.
		/// </summary>
		private readonly IRepositoryPopulator _repoPopulator;

		/// <summary>
		/// Performs operations upon pushes to a student repositories.
		/// </summary>
		private readonly IPushEventRetriever _pushEventRetriever;

		/// <summary>
		/// Creates build jobs for a given set of commits.
		/// </summary>
		private readonly IPushEventProcessor _pushEventProcessor;

		/// <summary>
		/// Validates payloads from GitHub webhooks.
		/// </summary>
		private readonly IGitHubWebhookValidator _webhookValidator;

		/// <summary>
		/// The JSON serializer.
		/// </summary>
		private readonly IJsonSerializer _jsonSerializer;

		/// <summary>
		/// The time provider.
		/// </summary>
		private readonly ITimeProvider _timeProvider;

		/// <summary>
		/// Constructor.
		/// </summary>
		public ProjectService(
			DatabaseContext dbContext, 
			IRepositoryPopulator repoPopulator,
			IPushEventRetriever pushEventRetriever,
			IPushEventProcessor pushEventProcessor,
			IGitHubWebhookValidator webhookValidator,
			IJsonSerializer jsonSerializer,
			ITimeProvider timeProvider)
		{
			_dbContext = dbContext;
			_repoPopulator = repoPopulator;
			_pushEventRetriever = pushEventRetriever;
			_pushEventProcessor = pushEventProcessor;
			_webhookValidator = webhookValidator;
			_jsonSerializer = jsonSerializer;
			_timeProvider = timeProvider;
		}

		/// <summary>
		/// Returns the list of projects.
		/// </summary>
		public async Task<IList<Project>> GetProjectsAsync(string classroomName)
		{
			var classroom = await LoadClassroomAsync(classroomName);

			return await _dbContext.Projects
				.Where(project => project.ClassroomId == classroom.Id)
				.ToListAsync();
		}

		/// <summary>
		/// Returns the project with the given name.
		/// </summary>
		public async Task<Project> GetProjectAsync(string classroomName, string projectName)
		{
			var classroom = await LoadClassroomAsync(classroomName);

			// Load all checkpoint due dates separately, due to missing functionality in EF
			await _dbContext.Checkpoints
				.Where(c => c.Project.Name == projectName)
				.Include(c => c.SectionDates)
				.Include(c => c.TestClasses)
				.ToListAsync();

			return await _dbContext.Projects
				.Where(project => project.ClassroomId == classroom.Id)
				.Include(project => project.Classroom)
				.Include(project => project.TestClasses)
				.Include(project => project.PrivateFilePaths)
				.Include(project => project.ImmutableFilePaths)
				.SingleOrDefaultAsync(project => project.Name == projectName);
		}

		/// <summary>
		/// Returns the project status for each active project,
		/// for the given user.
		/// </summary>
		public async Task<ProjectStatusResults> GetProjectStatusAsync(
			string classroomName,
			int userId)
		{
			var user = await _dbContext.Users
				.Where(u => u.Id == userId)
				.Include(u => u.ClassroomMemberships)
					.ThenInclude(cm => cm.Classroom)
				.SingleOrDefaultAsync();

			var classroomMembership = user.ClassroomMemberships
				.Single(cm => cm.Classroom.Name == classroomName);

			var testCountData = await _dbContext.Builds
				.Where(build => build.Commit.Project.Classroom.Name == classroomName)
				.Where(build => build.Commit.Project.ExplicitSubmissionRequired)
				.Where(build => build.Commit.UserId == userId)
				.Select
				(
					build => new
					{
						ProjectName = build.Commit.Project.Name,
						BuildStatus = build.Status,
						build.Id,
						build.Commit.PushDate,
						build.Commit.CommitDate,
						TestsPassing = build.TestResults.Count(tr => tr.Succeeded),
						TestsFailing = build.TestResults.Count(tr => !tr.Succeeded)
					}
				).ToListAsync();

			var projects = testCountData
				.GroupBy
				(
					tc => tc.ProjectName
				)
				.Select
				(
					group => new
					{
						ProjectName = group.Key,

						LastBuild = testCountData
							.Where(tc => tc.ProjectName == group.Key)
							.OrderBy(tc => tc.PushDate)
							.ThenBy(tc => tc.CommitDate)
							.LastOrDefault(),

						CompletedBuilds = testCountData
							.Where(tc => tc.ProjectName == group.Key)
							.Where(tc => tc.BuildStatus == BuildStatus.Completed)
							.OrderBy(tc => tc.PushDate)
							.ThenBy(tc => tc.CommitDate)
							.ToList()
					}
				)
				.OrderByDescending
				(
					project => project.LastBuild?.PushDate ?? DateTime.MinValue
				).ToList();

			var projectStatus = projects
				.Select
				(
					project => new ProjectStatus
					(
						project.ProjectName,
						$"{project.ProjectName}_{classroomMembership.GitHubTeam}",
						project.LastBuild.PushDate,
						project.LastBuild.BuildStatus == BuildStatus.Completed,
						project.CompletedBuilds.Select
						(
							build => new BuildTestCount
							(
								build.Id,
								build.PushDate,
								build.TestsPassing,
								build.TestsFailing
							)
						)?.ToList()
					)
				).ToList();

			return new ProjectStatusResults
			(
				user.LastName,
				user.FirstName,
				user.Id,
				projectStatus
			);
		}

		/// <summary>
		/// Creates a project.
		/// </summary>
		public async Task CreateProjectAsync(string classroomName, Project project)
		{
			var classroom = await LoadClassroomAsync(classroomName);

			UpdateProject(project);

			project.ClassroomId = classroom.Id;
			_dbContext.Add(project);

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Updates a project.
		/// </summary>
		public async Task UpdateProjectAsync(string classroomName, Project project)
		{
			var classroom = await LoadClassroomAsync(classroomName);

			UpdateProject(project);

			project.ClassroomId = classroom.Id;
			_dbContext.Update(project);

			await _dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Removes a project.
		/// </summary>
		public async Task DeleteProjectAsync(string classroomName, string projectName)
		{
			var project = await GetProjectAsync(classroomName, projectName);
			_dbContext.Projects.Remove(project);

			await _dbContext.SaveChangesAsync();
		}
		
		/// <summary>
		/// Creates student repositories for all students in a given section,
		/// based on the contents of the project template.
		/// </summary>
		public async Task<IList<CreateStudentRepoResult>> CreateStudentRepositoriesAsync(
			string classroomName,
			string projectName,
			string sectionName, 
			string webhookUrl,
			bool overwriteIfSafe)
		{
			var project = await GetProjectAsync(classroomName, projectName);

			var section = await _dbContext.Sections
				.Where(s => s.ClassroomId == project.ClassroomId)
				.Where(s => s.Name == sectionName)
				.SingleAsync();

			List<ClassroomMembership> students = await GetStudentsAsync(section);

			return await _repoPopulator.CreateReposAsync
			(
				project,
				students,
				webhookUrl,
				overwriteIfSafe
			);
		}

		/// <summary>
		/// Returns a list of students in the given section.
		/// </summary>
		private async Task<List<ClassroomMembership>> GetStudentsAsync(Section section)
		{
			return await _dbContext.ClassroomMemberships
				.Where
				(
					cm => cm.SectionMemberships.Any
					(
						sm => sm.SectionId == section.Id
						   && sm.Role == SectionRole.Student
					)
				).Include(cm => cm.User)
				.ToListAsync();
		}

		/// <summary>
		/// Returns a list of students in the given classroom.
		/// </summary>
		private async Task<List<ClassroomMembership>> GetStudentsAsync(Classroom classroom)
		{
			return await _dbContext.ClassroomMemberships
				.Where
				(
					cm => cm.SectionMemberships.Any
					(
						sm => sm.ClassroomMembership.ClassroomId == classroom.Id
						   && sm.Role == SectionRole.Student
					)
				).Include(cm => cm.User)
				.ToListAsync();
		}

		/// <summary>
		/// Returns a list of students in the given classroom.
		/// </summary>
		private async Task<ClassroomMembership> GetStudentAsync(
			Classroom classroom, 
			int userId)
		{
			return await _dbContext.ClassroomMemberships
				.Where(cm => cm.ClassroomId == classroom.Id)
				.Where(cm => cm.UserId == userId)
				.Include(cm => cm.User)
				.SingleOrDefaultAsync();
		}

		/// <summary>
		/// Verifies that a GitHub webhook payload is correctly signed.
		/// </summary>
		public bool VerifyGitHubWebhookPayloadSigned(byte[] content, string signature)
		{
			return _webhookValidator.VerifyWebhookPayloadSigned(content, signature);
		}

		/// <summary>
		/// Returns a list of files in a project template repository.
		/// </summary>
		public async Task<IList<ProjectRepositoryFile>> GetTemplateFileListAsync(
			string classroomName,
			string projectName)
		{
			var project = await GetProjectAsync(classroomName, projectName);

			return await _repoPopulator.GetRepoFileListAsync(project);
		}

		/// <summary>
		/// Called when a push event is received from a GitHub web hook,
		/// to queue a build of the commit that was pushed.
		/// </summary>
		public async Task OnRepositoryPushAsync(
			string classroomName, 
			string serializedPushEvent,
			string buildResultCallbackUrl)
		{
			var pushEvent = _jsonSerializer
				.Deserialize<GitHubPushEvent>(serializedPushEvent);

			pushEvent.CreatedAt = _timeProvider.UtcNow;

			bool defaultBranchPush = string.Equals
			(
				pushEvent.Ref,
				$"refs/heads/{pushEvent.Repository.Default_Branch}"
			);

			if (!defaultBranchPush)
			{
				return;
			}

			var repoNameParts = pushEvent.Repository.Name.Split('_');
			var projectName = repoNameParts[0];
			var gitHubTeamName = repoNameParts[1];

			var project = await GetProjectAsync(classroomName, projectName);

			var classroomMembership = await _dbContext.ClassroomMemberships
				.Where(cm => cm.ClassroomId == project.Classroom.Id)
				.Where(cm => cm.GitHubTeam == gitHubTeamName)
				.SingleOrDefaultAsync();

			var commits = await _dbContext.Commits
				.Where(c => c.ProjectId == project.Id)
				.Where(c => c.UserId == classroomMembership.UserId)
				.ToListAsync();

			var existingCommits = new HashSet<CommitDescriptor>
			(
				commits.Select(c => new CommitDescriptor(c.Sha, c.ProjectId, c.UserId))
			);

			await ProcessPushEventsAsync
			(
				project,
				existingCommits,
				new List<StudentRepoPushEvents>()
				{
					new StudentRepoPushEvents
					(
						classroomMembership,
						new List<GitHubPushEvent>() {pushEvent}
					)
				},
				buildResultCallbackUrl
			);
		}

		/// <summary>
		/// Returns the classroom with the given name.
		/// </summary>
		private async Task<Classroom> LoadClassroomAsync(string classroomName)
		{
			return await _dbContext.Classrooms
				.Where(c => c.Name == classroomName)
				.Include(c => c.Sections)
				.SingleOrDefaultAsync();
		}

		/// <summary>
		/// Checks for missed push events for all students.
		/// Returns false if the project does not exist.
		/// </summary>
		public async Task<bool> ProcessMissedCommitsForAllStudentsAsync(
			string classroomName,
			string projectName,
			string buildResultCallbackUrl)
		{
			var project = await GetProjectAsync(classroomName, projectName);
			if (project == null)
			{
				return false;
			}

			var students = await GetStudentsAsync(project.Classroom);

			await ProcessMissedCommitsAsync
			(
				buildResultCallbackUrl, 
				project, 
				students
			);

			return true;
		}
		
		/// <summary>
		/// Checks for missed push events for a single student.
		/// Returns false if the project or student does not exist.
		/// </summary>
		public async Task<bool> ProcessMissedCommitsForStudentAsync(
			string classroomName,
			string projectName,
			int userId,
			string buildResultCallbackUrl)
		{
			var project = await GetProjectAsync(classroomName, projectName);
			if (project == null)
			{
				return false;
			}

			var student = await GetStudentAsync(project.Classroom, userId);
			if (student == null)
			{
				return false;
			}

			await ProcessMissedCommitsAsync
			(
				buildResultCallbackUrl,
				project, 
				new List<ClassroomMembership>() {student}
			);

			return true;
		}

		/// <summary>
		/// Checks for missed push events for a list of students.
		/// </summary>
		private async Task ProcessMissedCommitsAsync(
			string buildResultCallbackUrl, 
			Project project, 
			IList<ClassroomMembership> students)
		{
			var pushEvents = await _pushEventRetriever
				.GetAllPushEventsAsync(project, students);

			var commits = await _dbContext.Commits
				.Where(commit => commit.ProjectId == project.Id)
				.ToListAsync();

			var existingCommits = new HashSet<CommitDescriptor>
			(
				commits.Select
				(
					c => new CommitDescriptor(c.Sha, c.ProjectId, c.UserId)
				)
			);

			await ProcessPushEventsAsync
			(
				project,
				existingCommits,
				pushEvents,
				buildResultCallbackUrl
			);
		}

		/// <summary>
		/// Updates a project.
		/// </summary>
		private void UpdateProject(Project project)
		{
			UpdateTestClassOrder(project.TestClasses);

			_dbContext.RemoveUnwantedObjects
			(
				_dbContext.TestClasses,
				testClass => testClass.Id,
				testClass => testClass.ProjectId == project.Id,
				project.TestClasses
			);

			_dbContext.RemoveUnwantedObjects
			(
				_dbContext.PrivateFilePaths,
				path => path.Id,
				path => path.ProjectId == project.Id,
				project.PrivateFilePaths
			);

			_dbContext.RemoveUnwantedObjects
			(
				_dbContext.ImmutableFilePaths,
				path => path.Id,
				path => path.ProjectId == project.Id,
				project.ImmutableFilePaths
			);
		}

		/// <summary>
		/// Updates the order of test classes.
		/// </summary>
		private void UpdateTestClassOrder(List<TestClass> projectTestClasses)
		{
			if (projectTestClasses != null)
			{
				int index = 0;
				foreach (var testClass in projectTestClasses)
				{
					testClass.Order = index;
					index++;
				}
			}
		}

		/// <summary>
		/// Processes push events received from GitHub.
		/// </summary>
		private async Task ProcessPushEventsAsync(
			Project project,
			ICollection<CommitDescriptor> existingCommits,
			IList<StudentRepoPushEvents> pushEvents,
			string buildResultCallbackUrl)
		{
			var newCommits = _pushEventProcessor.GetNewCommitsToProcess
			(
				project,
				existingCommits,
				pushEvents
			);

			foreach (var pushEventCommit in newCommits)
			{
				_dbContext.Commits.Add(pushEventCommit.Commit);
			}

			await _dbContext.SaveChangesAsync();

			if (project.ExplicitSubmissionRequired)
			{
				foreach (var pushEventCommit in newCommits)
				{
					var jobId = await _pushEventProcessor.CreateBuildJobAsync
					(
						project,
						pushEventCommit,
						buildResultCallbackUrl
					);

					pushEventCommit.Commit.BuildJobId = jobId;
				}
			}

			await _dbContext.SaveChangesAsync();
		}
	}
}
