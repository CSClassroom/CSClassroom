using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.BuildService.Model.ProjectRunner;
using CSC.Common.Infrastructure.Queue;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Repository;
using Microsoft.EntityFrameworkCore;
using TestResult = CSC.CSClassroom.Model.Projects.TestResult;

namespace CSC.CSClassroom.Service.Projects
{
	/// <summary>
	/// Performs build operations.
	/// </summary>
	public class BuildService : IBuildService
	{
		/// <summary>
		/// The database context.
		/// </summary>
		private readonly DatabaseContext _dbContext;

		/// <summary>
		/// The job queue client.
		/// </summary>
		private readonly IJobQueueClient _jobQueueClient;

		/// <summary>
		/// Provides the current time.
		/// </summary>
		private readonly ITimeProvider _timeProvider;

		/// <summary>
		/// The default estimate for a build, in seconds.
		/// </summary>
		private const int c_defaultEstimate = 60;

		/// <summary>
		/// Constructor.
		/// </summary>
		public BuildService(
			DatabaseContext dbContext, 
			IJobQueueClient jobQueueClient, 
			ITimeProvider timeProvider)
		{
			_dbContext = dbContext;
			_jobQueueClient = jobQueueClient;
			_timeProvider = timeProvider;
		}

		/// <summary>
		/// Returns the list of project builds for a given user.
		/// </summary>
		public async Task<IList<Build>> GetUserBuildsAsync(
			string classroomName,
			string projectName,
			int userId)
		{
			var project = await LoadProjectAsync(classroomName, projectName);

			return await GetBuildsDescending(project)
				.Where(build => build.Commit.UserId == userId)
				.ToListAsync();
		}
		
		/// <summary>
		/// Returns the latest build for each student in the given section,
		/// for a given project.
		/// </summary>
		public async Task<IList<Build>> GetSectionBuildsAsync(
			string classroomName,
			string projectName,
			string sectionName)
		{
			var project = await LoadProjectAsync(classroomName, projectName);
			var section = await LoadSectionAsync(classroomName, sectionName);

			var allSectionBuilds = await _dbContext.Builds
				.Where
				(
					build => build.Commit.User.ClassroomMemberships.Any
					(
						cm => cm.SectionMemberships.Any
						(
							sm => sm.SectionId == section.Id
								&& sm.Role == SectionRole.Student
						)
					)
				)
				.Where
				(
					build => build.Commit.ProjectId == project.Id
				)
				.Include(build => build.Commit)
				.ToListAsync();

			var buildIds = allSectionBuilds
				.GroupBy
				(
					build => build.Commit.UserId
				)
				.Select
				(
					group => group.OrderByDescending(build => build.Commit.PushDate)
						.ThenByDescending(build => build.Commit.CommitDate)
						.FirstOrDefault()
						.Id
				).ToList();

			return await _dbContext.Builds
				.Where(build => buildIds.Contains(build.Id))
				.Include(build => build.Commit.User)
				.Include(build => build.TestResults)
				.OrderByDescending(build => build.Commit.PushDate)
				.ToListAsync();
		}
		
		/// <summary>
		/// Returns the latest build for the given user and project.
		/// </summary>
		public async Task<LatestBuildResult> GetLatestBuildResultAsync(
			string classroomName, 
			string projectName, 
			int userId)
		{
			var project = await LoadProjectAsync(classroomName, projectName);
			var commit = await GetLatestCommitAsync(project, userId);

			if (commit == null)
			{
				return null;
			}
			else if (commit.Build == null)
			{
				var estimatedDuration = await GetEstimatedBuildDurationAsync(project, userId);

				return new LatestBuildResult(commit, null /*buildResult*/, estimatedDuration);
			}
			else
			{
				var buildResult = await GetBuildResultAsync(commit.Build);

				return new LatestBuildResult(commit, buildResult, null /*estimatedDuration*/);
			}
		}

		/// <summary>
		/// Returns the build with the given id.
		/// </summary>
		public async Task<BuildResult> GetBuildResultAsync(
			string classroomName,
			string projectName, 
			int buildId)
		{
			var build = await _dbContext.Builds
				.Where(b => b.Commit.Project.Classroom.Name == classroomName)
				.Where(b => b.Commit.Project.Name == projectName)
				.Include(b => b.Commit)
				.Include(b => b.Commit.User)
				.Include(b => b.Commit.User.ClassroomMemberships)
				.Include(b => b.Commit.Project)
				.Include(b => b.Commit.Project.Classroom)
				.Include(b => b.TestResults)
				.SingleOrDefaultAsync(b => b.Id == buildId);

			if (build == null)
			{
				return null;
			}

			return await GetBuildResultAsync(build);
		}

		/// <summary>
		/// Returns a query for the builds of a given user, in ascending order.
		/// </summary>
		private IOrderedQueryable<Build> GetBuildsDescending(Project project)
		{
			return _dbContext.Builds
				.Where(build => build.Commit.ProjectId == project.Id)
				.Include(build => build.Commit)
				.Include(build => build.Commit.User.ClassroomMemberships)
				.Include(build => build.TestResults)
				.OrderByDescending(build => build.Commit.PushDate)
				.ThenByDescending(build => build.Commit.CommitDate);
		}

		/// <summary>
		/// Returns a list of the number of pass/failed tests for each build,
		/// in ascending order of push date.
		/// </summary>
		public async Task<IList<BuildTestCount>> GetBuildTestCountsAsync(Project project, int userId)
		{
			return await _dbContext.Builds
				.Where(build => build.Status == BuildStatus.Completed)
				.Where(build => build.Commit.ProjectId == project.Id)
				.Where(build => build.Commit.UserId == userId)
				.OrderBy(build => build.Commit.PushDate)
				.ThenBy(build => build.Commit.CommitDate)
				.Select
				(
					build => new BuildTestCount
					(
						build.Id,
						build.Commit.PushDate,
						build.TestResults.Count(tr => tr.Succeeded),
						build.TestResults.Count(tr => !tr.Succeeded)
					)
				).ToListAsync();
		}

		/// <summary>
		/// Returns a build result for the given build.
		/// </summary>
		private async Task<BuildResult> GetBuildResultAsync(Build build)
		{
			var project = build.Commit.Project;

			var isLatestBuild = await IsLatestBuildAsync(project, build);

			var submissions = await _dbContext.Submissions
				.Where(submission => submission.Commit.ProjectId == project.Id)
				.Include(submission => submission.Commit)
				.Include(submission => submission.Checkpoint)
				.Where(submission => submission.Commit.UserId == build.Commit.UserId)
				.ToListAsync();

			var checkpoints = await _dbContext.Checkpoints
				.Where(checkpoint => checkpoint.ProjectId == project.Id)
				.Include(checkpoint => checkpoint.SectionDates)
				.ToListAsync();

			var section = await _dbContext.SectionMemberships
				.Where
				(
					sm => sm.ClassroomMembership.UserId == build.Commit.User.Id
						&& sm.ClassroomMembership.ClassroomId == build.Commit.Project.Classroom.Id
						&& sm.Role == SectionRole.Student
				)
				.Select(sm => sm.Section)
				.FirstOrDefaultAsync();

			var testCounts = await GetBuildTestCountsAsync
			(
				build.Commit.Project,
				build.Commit.UserId
			);

			return new BuildResult
			(
				build,
				isLatestBuild,
				section,
				checkpoints,
				submissions,
				testCounts
			);
		}

		/// <summary>
		/// Returns the latest commit for the given user.
		/// </summary>
		public async Task<Commit> GetLatestCommitAsync(Project project, int userId)
		{
			return await GetCommitsDescending(project, userId)
				.FirstOrDefaultAsync();
		}

		/// <summary>
		/// Returns a list of commits, in descending order.
		/// </summary>
		private IQueryable<Commit> GetCommitsDescending(Project project, int userId)
		{
			return _dbContext.Commits
				.Where(commit => commit.ProjectId == project.Id)
				.Where(commit => commit.UserId == userId)
				.Include(commit => commit.Build)
				.Include(commit => commit.Build.TestResults)
				.Include(commit => commit.User)
				.Include(commit => commit.User.ClassroomMemberships)
				.Include(commit => commit.Project.Classroom)
				.OrderByDescending(commit => commit.PushDate)
				.ThenByDescending(commit => commit.CommitDate);
		}

		/// <summary>
		/// Called by the build service when a build completes, to store the result.
		/// </summary>
		public async Task OnBuildCompletedAsync(ProjectJobResult projectJobResult)
		{
			var commit = await _dbContext.Commits
				.Where(c => c.BuildRequestToken == projectJobResult.BuildRequestToken)
				.Include(c => c.Build)
				.Include(c => c.Project)
				.SingleOrDefaultAsync();

			if (commit == null)
			{
				return;
			}

			var previousBuild = await GetBuildsDescending(commit.Project)
				.Where(build => build.Commit.UserId == commit.UserId)
				.Where(build => build.Commit.PushDate < commit.PushDate)
				.Where(build => build.Status == BuildStatus.Completed)
				.FirstOrDefaultAsync();

			var previousTestResults = previousBuild?.TestResults
				?.ToDictionary(tr => $"{tr.ClassName}.{tr.TestName}", tr => tr);

			commit.Build = new Build()
			{
				CommitId = commit.Id,
				Commit = commit,
				DateStarted = projectJobResult.JobStartedDate,
				DateCompleted = projectJobResult.JobFinishedDate,
				Output = projectJobResult.BuildOutput,
				Status = GetBuildStatus(projectJobResult.Status),
				TestResults = projectJobResult?.TestResults
					?.Select
					(
						tr => new Model.Projects.TestResult()
						{
							ClassName = tr.ClassName,
							TestName = tr.TestName,
							Succeeded = tr.Succeeded,
							PreviouslySucceeded = GetPreviouslySucceeded(tr, previousTestResults),
							FailureMessage = tr.Failure?.Message,
							FailureOutput = tr.Failure?.Output,
							FailureTrace = tr.Failure?.Trace
						}
					)?.ToList()
			};

			await _dbContext.SaveChangesAsync();
		}
		
		/// <summary>
		/// Returns whether a test previously succeeded.
		/// </summary>
		private bool GetPreviouslySucceeded(
			CSC.BuildService.Model.ProjectRunner.TestResult testResult, 
			IDictionary<string, TestResult> previousTestResults)
		{
			if (previousTestResults == null)
				return false;

			TestResult prevTestResult;
			previousTestResults.TryGetValue
			(
				$"{testResult.ClassName}.{testResult.TestName}", 
				out prevTestResult
			);

			if (prevTestResult != null)
			{
				return prevTestResult.Succeeded;
			}

			return false;
		}

		/// <summary>
		/// Returns the build status for a corresponding project job.
		/// </summary>
		private BuildStatus GetBuildStatus(ProjectJobStatus status)
		{
			switch (status)
			{
				case ProjectJobStatus.Completed:
					return BuildStatus.Completed;

				case ProjectJobStatus.Error:
					return BuildStatus.Error;

				case ProjectJobStatus.Timeout:
					return BuildStatus.Timeout;

				default:
					throw new ArgumentOutOfRangeException(nameof(status));
			}
		}

		/// <summary>
		/// Returns whether or not the given build is the latest build.
		/// </summary>
		private async Task<bool> IsLatestBuildAsync(Project project, Build build)
		{
			var latestBuildId = await GetBuildsDescending(project)
				.Where(b => b.Commit.UserId == build.Commit.UserId)
				.Select(b => b.Id)
				.FirstAsync();

			return build.Id == latestBuildId;
		}

		/// <summary>
		/// Return the estimated build duration.
		/// </summary>
		private async Task<TimeSpan> GetEstimatedBuildDurationAsync(Project project, int userId)
		{
			var duration = await GetBuildsDescending(project)
				.Where(build => build.Commit.UserId == userId)
				.Where(build => build.Status == BuildStatus.Completed)
				.Select(build => build.DateCompleted - build.DateStarted)
				.FirstOrDefaultAsync();

			if (duration == default(TimeSpan))
			{
				return TimeSpan.FromSeconds(c_defaultEstimate);
			}

			return duration;
		}

		/// <summary>
		/// Monitors the progress of a job.
		/// </summary>
		public async Task<BuildProgress> MonitorProgressAsync(
			string classroomName,
			string projectName, 
			int userId)
		{
			var project = await LoadProjectAsync(classroomName, projectName);

			var asyncEnumerable = GetCommitsDescending(project, userId).ToAsyncEnumerable();
			var asyncEnumerator = asyncEnumerable.GetEnumerator();

			while (await asyncEnumerator.MoveNext())
			{
				var commit = asyncEnumerator.Current;

				if (commit.Build != null)
				{
					return BuildProgress.CompletedBuild();
				}

				if (commit.BuildJobId != null)
				{
					var jobStatus = await _jobQueueClient
						.GetJobStatusAsync(commit.BuildJobId);

					if (jobStatus.State == JobState.NotStarted)
					{
						return BuildProgress.EnqueuedBuild();
					}
					else if (jobStatus.State == JobState.InProgress)
					{
						var duration = _timeProvider.UtcNow - jobStatus.EnteredState;

						return BuildProgress.InProgressBuild(duration);
					}
					else
					{
						return BuildProgress.UnknownBuild();
					}
				}
			}

			return null;		
		}

		/// <summary>
		/// Called by the build service when a build completes, to store the result.
		/// </summary>
		public async Task<TestResult> GetTestResultAsync(
			string classroomName,
			string projectName, 
			int testResultId)
		{
			return await _dbContext.TestResults
				.Where(tr => tr.Build.Commit.Project.Classroom.Name == classroomName)
				.Where(tr => tr.Build.Commit.Project.Name == projectName)
				.Where(tr => tr.Id == testResultId)
				.Include(tr => tr.Build)
				.Include(tr => tr.Build.Commit)
				.Include(tr => tr.Build.Commit.Project)
				.Include(tr => tr.Build.Commit.Project.TestClasses)
				.FirstOrDefaultAsync();
		}

		/// <summary>
		/// Loads a project from the database.
		/// </summary>
		private async Task<Project> LoadProjectAsync(
			string classroomName, 
			string projectName)
		{
			return await _dbContext.Projects
				.Where(p => p.Classroom.Name == classroomName)
				.SingleAsync(p => p.Name == projectName);
		}

		/// <summary>
		/// Loads a project from the database.
		/// </summary>
		private async Task<Section> LoadSectionAsync(
			string classroomName,
			string sectionName)
		{
			return await _dbContext.Sections
				.Where(s => s.Classroom.Name == classroomName)
				.SingleAsync(s => s.Name == sectionName);
		}
	}
}
