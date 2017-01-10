using System;
using System.Linq;
using System.Threading.Tasks;
using CSC.BuildService.Model.ProjectRunner;
using CSC.Common.Infrastructure.Queue;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Projects;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using TestResult = CSC.CSClassroom.Model.Projects.TestResult;

namespace CSC.CSClassroom.Service.UnitTests.Projects
{
	/// <summary>
	/// Unit tests for the build service.
	/// </summary>
	public class BuildService_UnitTests
	{
		/// <summary>
		/// Ensures that GetUserBuildsAsync returns the builds for a given
		/// project/user.
		/// </summary>
		[Fact]
		public async Task GetUserBuildsAsync_ReturnsBuilds()
		{
			var database = GetDatabaseBuilderWithBuilds().Build();

			var userId = database.Context.Users
				.Single(u => u.UserName == "Student1")
				.Id;

			database.Reload();

			var buildService = GetBuildService(database.Context);

			var results = await buildService.GetUserBuildsAsync
			(
				"Class1", 
				"Project1", 
				userId
			);

			Assert.Equal(3, results.Count);

			Assert.Equal("Student1", results[0].Commit.User.UserName);
			Assert.Equal("Commit3", results[0].Commit.Sha);
			Assert.Equal(BuildStatus.Completed, results[0].Status);
			Assert.Equal(2, results[0].TestResults.Count);

			Assert.Equal("Student1", results[1].Commit.User.UserName);
			Assert.Equal("Commit2", results[1].Commit.Sha);
			Assert.Equal(BuildStatus.Error, results[1].Status);
			Assert.Equal(0, results[1].TestResults.Count);

			Assert.Equal("Student1", results[2].Commit.User.UserName);
			Assert.Equal("Commit1", results[2].Commit.Sha);
			Assert.Equal(BuildStatus.Completed, results[2].Status);
			Assert.Equal(2, results[2].TestResults.Count);
		}

		/// <summary>
		/// Ensures that GetSectionBuildsAsync returns the latest build
		/// for each student in the given section.
		/// </summary>
		[Fact]
		public async Task GetSectionBuildsAsync_ReturnsBuilds()
		{
			var database = GetDatabaseBuilderWithBuilds().Build();

			var buildService = GetBuildService(database.Context);

			var results = await buildService.GetSectionBuildsAsync
			(
				"Class1",
				"Project1",
				"Period1"
			);

			Assert.Equal(3, results.Count);

			Assert.Equal("Student1", results[0].Commit.User.UserName);
			Assert.Equal("Commit3", results[0].Commit.Sha);
			Assert.Equal(BuildStatus.Completed, results[0].Status);
			Assert.Equal(2, results[0].TestResults.Count);

			Assert.Equal("Student3", results[1].Commit.User.UserName);
			Assert.Equal("Commit2", results[1].Commit.Sha);
			Assert.Equal(BuildStatus.Error, results[1].Status);
			Assert.Equal(0, results[1].TestResults.Count);

			Assert.Equal("Student2", results[2].Commit.User.UserName);
			Assert.Equal("Commit1", results[2].Commit.Sha);
			Assert.Equal(BuildStatus.Error, results[2].Status);
			Assert.Equal(0, results[2].TestResults.Count);
		}

		/// <summary>
		/// Ensures that GetLatestBuildResultAsync returns null when there is 
		/// no build for the user/project.
		/// </summary>
		[Fact]
		public async Task GetLatestBuildResultAsync_NoBuild_ReturnsNull()
		{
			var database = GetDatabaseBuilderWithBuilds().Build();

			var userId = database.Context.Users
				.Single(u => u.UserName == "Student4")
				.Id;

			database.Reload();

			var buildService = GetBuildService(database.Context);

			var latestBuildResult = await buildService.GetLatestBuildResultAsync
			(
				"Class1",
				"Project1",
				userId
			);

			Assert.Null(latestBuildResult);
		}

		/// <summary>
		/// Ensures that GetLatestBuildResultAsync returns the latest build, 
		/// when that build is in progress.
		/// </summary>
		[Fact]
		public async Task GetLatestBuildResultAsync_InProgress_ReturnsInProgressBuildResult()
		{
			var database = GetDatabaseBuilderWithBuilds().Build();

			var userId = database.Context.Users
				.Single(u => u.UserName == "Student3")
				.Id;

			database.Reload();

			var buildService = GetBuildService(database.Context);

			var latestBuildResult = await buildService.GetLatestBuildResultAsync
			(
				"Class1",
				"Project1",
				userId
			);

			Assert.Equal("Student3", latestBuildResult.Commit.User.UserName);
			Assert.Equal("Commit3", latestBuildResult.Commit.Sha);
			Assert.Null(latestBuildResult.BuildResult);
			Assert.Equal(SuccessfulBuildDuration, latestBuildResult.EstimatedBuildDuration.Value);
		}

		/// <summary>
		/// Ensures that GetLatestBuildResultAsync returns the latest build, 
		/// when that build has completed.
		/// </summary>
		[Fact]
		public async Task GetLatestBuildResultAsync_Completed_ReturnsCompletedBuildResult()
		{
			var database = GetDatabaseBuilderWithBuilds().Build();

			var userId = database.Context.Users
				.Single(u => u.UserName == "Student1")
				.Id;

			database.Reload();

			var buildService = GetBuildService(database.Context);

			var latestBuildResult = await buildService.GetLatestBuildResultAsync
			(
				"Class1",
				"Project1",
				userId
			);

			Assert.Equal("Student1", latestBuildResult.Commit.User.UserName);
			Assert.Equal("Commit3", latestBuildResult.Commit.Sha);
			Assert.True(latestBuildResult.BuildResult.LatestBuild);
			Assert.Equal(BuildStatus.Completed, latestBuildResult.BuildResult.Build.Status);
			Assert.Equal(2, latestBuildResult.BuildResult.Build.TestResults.Count);
			
			Assert.Equal(2, latestBuildResult.BuildResult.AllBuildTestCounts.Count);
			Assert.Equal(Commit1PushDate, latestBuildResult.BuildResult.AllBuildTestCounts[0].PushDate);
			Assert.Equal(1, latestBuildResult.BuildResult.AllBuildTestCounts[0].FailedCount);
			Assert.Equal(1, latestBuildResult.BuildResult.AllBuildTestCounts[0].PassedCount);
			Assert.Equal(Commit3PushDate, latestBuildResult.BuildResult.AllBuildTestCounts[1].PushDate);
			Assert.Equal(0, latestBuildResult.BuildResult.AllBuildTestCounts[1].FailedCount);
			Assert.Equal(2, latestBuildResult.BuildResult.AllBuildTestCounts[1].PassedCount);

			Assert.Equal(1, latestBuildResult.BuildResult.Submissions.SubmissionResults.Count);
			Assert.Equal("Checkpoint1", latestBuildResult.BuildResult.Submissions.SubmissionResults[0].CheckpointName);
			Assert.Equal("Commit3", latestBuildResult.BuildResult.Submissions.SubmissionResults[0].Submission.Commit.Sha);
		}

		/// <summary>
		/// Ensures that GetBuildResultAsync returns a result, when given
		/// a valid build ID.
		/// </summary>
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task GetBuildResultAsync_ValidBuildId_ReturnsResult(
			bool latestBuild)
		{
			var database = GetDatabaseBuilderWithBuilds().Build();
			var commitSha = latestBuild ? "Commit3" : "Commit1";

			var userId = database.Context.Users
				.Single(u => u.UserName == "Student1")
				.Id;

			var buildId = database.Context.Builds
				.Where(b => b.Commit.UserId == userId)
				.Single(b => b.Commit.Sha == commitSha)
				.Id;

			database.Reload();

			var buildService = GetBuildService(database.Context);

			var buildResult = await buildService.GetBuildResultAsync
			(
				"Class1",
				"Project1",
				buildId
			);

			Assert.Equal("Student1", buildResult.Build.Commit.User.UserName);
			Assert.Equal(commitSha, buildResult.Build.Commit.Sha);
			Assert.Equal(latestBuild, buildResult.LatestBuild);
			Assert.Equal(BuildStatus.Completed, buildResult.Build.Status);
			Assert.Equal(2, buildResult.Build.TestResults.Count);

			Assert.Equal(2, buildResult.AllBuildTestCounts.Count);
			Assert.Equal(Commit1PushDate, buildResult.AllBuildTestCounts[0].PushDate);
			Assert.Equal(1, buildResult.AllBuildTestCounts[0].FailedCount);
			Assert.Equal(1, buildResult.AllBuildTestCounts[0].PassedCount);
			Assert.Equal(Commit3PushDate, buildResult.AllBuildTestCounts[1].PushDate);
			Assert.Equal(0, buildResult.AllBuildTestCounts[1].FailedCount);
			Assert.Equal(2, buildResult.AllBuildTestCounts[1].PassedCount);

			Assert.Equal(1, buildResult.Submissions.SubmissionResults.Count);
			Assert.Equal("Checkpoint1", buildResult.Submissions.SubmissionResults[0].CheckpointName);
			Assert.Equal("Commit3", buildResult.Submissions.SubmissionResults[0].Submission.Commit.Sha);
		}

		/// <summary>
		/// Ensures that GetBuildResultAsync returns null, when given an 
		/// invalid build id.
		/// </summary>
		[Fact]
		public async Task GetBuildResultAsync_InalidBuildId_ReturnsNull()
		{
			var database = GetDatabaseBuilderWithBuilds().Build();

			var buildService = GetBuildService(database.Context);

			var buildResult = await buildService.GetBuildResultAsync
			(
				"Class1",
				"Project1",
				buildId: 12345
			);

			Assert.Null(buildResult);
		}

		/// <summary>
		/// Ensures that OnBuildCompletedAsync does not process the build result when
		/// it contains an invalid build request token.
		/// </summary>
		[Fact]
		public async Task OnBuildCompletedAsync_InvalidBuildRequestToken_DoesNotProcessResult()
		{
			var database = GetDatabaseBuilderWithBuilds().Build();

			var buildService = GetBuildService(database.Context);

			int numBuildsBefore = database.Context.Builds.Count();
			database.Reload();

			await buildService.OnBuildCompletedAsync
			(
				new ProjectJobResult()
				{
					BuildRequestToken = "WrongToken"
				}
			);

			database.Reload();
			int numBuildsAfter = database.Context.Builds.Count();

			Assert.Equal(numBuildsBefore, numBuildsAfter);
		}

		/// <summary>
		/// Ensures that OnBuildCompletedAsync processes the build result when
		/// the build succeeded.
		/// </summary>
		[Fact]
		public async Task OnBuildCompletedAsync_BuildSucceeded_ProcessesBuildResult()
		{
			var database = GetDatabaseBuilderWithBuilds().Build();

			var buildService = GetBuildService(database.Context);

			await buildService.OnBuildCompletedAsync
			(
				new ProjectJobResult()
				{
					BuildRequestToken = "BuildRequestToken",
					JobStartedDate = Commit3PushDate,
					JobFinishedDate = Commit3PushDate + SuccessfulBuildDuration,
					Status = ProjectJobStatus.Completed,
					TestResults = Collections.CreateList
					(
						new CSC.BuildService.Model.ProjectRunner.TestResult()
						{
							ClassName = "TestClass1",
							TestName = "Test1"
						},
						new CSC.BuildService.Model.ProjectRunner.TestResult()
						{
							ClassName = "TestClass2",
							TestName = "Test2",
							Failure = new TestFailure()
							{
								Message = "Test2 Message",
								Output = "Test2 Output",
								Trace = "Test2 Trace"
							}
						}
					)
				}
			);

			database.Reload();

			var commit = database.Context.Commits
				.Where(c => c.User.UserName == "Student3")
				.Where(c => c.Sha == "Commit3")
				.Include(c => c.Build)
				.Include(c => c.Build.TestResults)
				.Single();

			Assert.NotNull(commit.Build);
			Assert.Equal(BuildStatus.Completed, commit.Build.Status);
			Assert.Equal(2, commit.Build.TestResults.Count);

			Assert.Equal("TestClass1", commit.Build.TestResults[0].ClassName);
			Assert.Equal("Test1", commit.Build.TestResults[0].TestName);
			Assert.True(commit.Build.TestResults[0].Succeeded);
			Assert.False(commit.Build.TestResults[0].PreviouslySucceeded);

			Assert.Equal("TestClass2", commit.Build.TestResults[1].ClassName);
			Assert.Equal("Test2", commit.Build.TestResults[1].TestName);
			Assert.False(commit.Build.TestResults[1].Succeeded);
			Assert.True(commit.Build.TestResults[1].PreviouslySucceeded);
			Assert.Equal("Test2 Message", commit.Build.TestResults[1].FailureMessage);
			Assert.Equal("Test2 Output", commit.Build.TestResults[1].FailureOutput);
			Assert.Equal("Test2 Trace", commit.Build.TestResults[1].FailureTrace);
		}

		/// <summary>
		/// Ensures that OnBuildCompletedAsync processes the build result when
		/// the build failed.
		/// </summary>
		[Fact]
		public async Task OnBuildCompletedAsync_BuildFailed_ProcessesBuildResult()
		{
			var database = GetDatabaseBuilderWithBuilds().Build();

			var buildService = GetBuildService(database.Context);

			await buildService.OnBuildCompletedAsync
			(
				new ProjectJobResult()
				{
					BuildRequestToken = "BuildRequestToken",
					JobStartedDate = Commit3PushDate,
					JobFinishedDate = Commit3PushDate + FailedBuildDuration,
					Status = ProjectJobStatus.Error,
					BuildOutput = "Build Output"
				}
			);

			database.Reload();

			var commit = database.Context.Commits
				.Where(c => c.User.UserName == "Student3")
				.Where(c => c.Sha == "Commit3")
				.Include(c => c.Build)
				.Include(c => c.Build.TestResults)
				.Single();

			Assert.NotNull(commit.Build);
			Assert.Equal(BuildStatus.Error, commit.Build.Status);
			Assert.Equal(0, commit.Build.TestResults.Count);
			Assert.Equal("Build Output", commit.Build.Output);
		}

		/// <summary>
		/// Ensures that MonitorProgressAsync returns the correct result when
		/// the latest build does not have an associated job.
		/// </summary>
		[Fact]
		public async Task MonitorProgressAsync_NoJob_ReturnsResult()
		{
			var database = GetDatabaseBuilderWithBuilds().Build();

			var userId = database.Context.Users
				.Single(u => u.UserName == "Student3")
				.Id;

			database.Reload();

			var jobQueueClient = GetMockJobQueueClient(JobState.NotFound);
			var buildService = GetBuildService
			(
				database.Context,
				jobQueueClient: jobQueueClient.Object
			);

			var result = await buildService.MonitorProgressAsync
			(
				"Class1",
				"Project1",
				userId
			);

			Assert.False(result.Enqueued);
			Assert.Null(result.Duration);
			Assert.False(result.Completed);
		}

		/// <summary>
		/// Ensures that MonitorProgressAsync returns the correct result when
		/// the latest build is queued, but has not yet started.
		/// </summary>
		[Fact]
		public async Task MonitorProgressAsync_BuildNotStarted_ReturnsResult()
		{
			var database = GetDatabaseBuilderWithBuilds().Build();

			var userId = database.Context.Users
				.Single(u => u.UserName == "Student3")
				.Id;

			database.Reload();

			var jobQueueClient = GetMockJobQueueClient(JobState.NotStarted);
			var buildService = GetBuildService
			(
				database.Context,
				jobQueueClient: jobQueueClient.Object
			);

			var result = await buildService.MonitorProgressAsync
			(
				"Class1",
				"Project1",
				userId
			);

			Assert.True(result.Enqueued);
			Assert.Null(result.Duration);
			Assert.False(result.Completed);
		}

		/// <summary>
		/// Ensures that MonitorProgressAsync returns the correct result when
		/// the latest build is currently building.
		/// </summary>
		[Fact]
		public async Task MonitorProgressAsync_CurrentlyBuilding_ReturnsResult()
		{
			var database = GetDatabaseBuilderWithBuilds().Build();

			var userId = database.Context.Users
				.Single(u => u.UserName == "Student3")
				.Id;

			database.Reload();

			var jobQueueClient = GetMockJobQueueClient
			(
				JobState.InProgress, 
				enteredState: DateTime.MinValue
			);

			var timeProvider = GetMockTimeProvider
			(
				DateTime.MinValue + TimeSpan.FromSeconds(5)
			);

			var buildService = GetBuildService
			(
				database.Context,
				jobQueueClient: jobQueueClient.Object,
				timeProvider: timeProvider.Object
			);

			var result = await buildService.MonitorProgressAsync
			(
				"Class1",
				"Project1",
				userId
			);

			Assert.True(result.Enqueued);
			Assert.Equal(5, result.Duration);
			Assert.False(result.Completed);
		}

		/// <summary>
		/// Ensures that MonitorProgressAsync returns the correct result when
		/// the latest build is complete.
		/// </summary>
		[Fact]
		public async Task MonitorProgressAsync_CompletedBuild_ReturnsResult()
		{
			var database = GetDatabaseBuilderWithBuilds().Build();

			var userId = database.Context.Users
				.Single(u => u.UserName == "Student1")
				.Id;

			database.Reload();

			var buildService = GetBuildService(database.Context);

			var result = await buildService.MonitorProgressAsync
			(
				"Class1", 
				"Project1", 
				userId
			);

			Assert.True(result.Enqueued);
			Assert.Null(result.Duration);
			Assert.True(result.Completed);
		}

		/// <summary>
		/// Ensures that MonitorProgressAsync returns the correct result when
		/// the latest build is in an unknown state.
		/// </summary>
		[Fact]
		public async Task MonitorProgressAsync_UnknownState_ReturnsResult()
		{
			var database = GetDatabaseBuilderWithBuilds().Build();

			var userId = database.Context.Users
				.Single(u => u.UserName == "Student3")
				.Id;

			database.Reload();

			var jobQueueClient = GetMockJobQueueClient(JobState.Unknown);
			var buildService = GetBuildService
			(
				database.Context,
				jobQueueClient: jobQueueClient.Object
			);

			var result = await buildService.MonitorProgressAsync
			(
				"Class1",
				"Project1",
				userId
			);

			Assert.False(result.Enqueued);
			Assert.Null(result.Duration);
			Assert.False(result.Completed);
		}

		/// <summary>
		/// Ensures that GetTestResultAsync returns the test result.
		/// </summary>
		[Fact]
		public async Task GetTestResultAsync_ReturnsTestResult()
		{
			var database = GetDatabaseBuilderWithBuilds().Build();

			var userId = database.Context.Users
				.Single(u => u.UserName == "Student1")
				.Id;

			var testResultId = database.Context.TestResults
				.Where(tr => tr.Build.Commit.User.UserName == "Student1")
				.Where(tr => tr.Build.Commit.Sha == "Commit3")
				.OrderBy(tr => tr.Id)
				.First()
				.Id;

			database.Reload();

			var buildService = GetBuildService(database.Context);

			var result = await buildService.GetTestResultAsync
			(
				"Class1",
				"Project1",
				testResultId
			);
			
			Assert.Equal(userId, result.Build.Commit.UserId);
			Assert.Equal("TestClass1", result.ClassName);
			Assert.Equal("Test1", result.TestName);
			Assert.True(result.Succeeded);
		}

		/// <summary>
		/// Returns a database with a project.
		/// </summary>
		private TestDatabaseBuilder GetDatabaseBuilderWithBuilds()
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddSection("Class1", "Period1")
				.AddStudent("Student1", "Last1", "First1", "Class1", "Period1", "GitHubUser")
				.AddStudent("Student2", "Last2", "First2", "Class1", "Period1", "GitHubUser")
				.AddStudent("Student3", "Last3", "First3", "Class1", "Period1", "GitHubUser")
				.AddStudent("Student4", "Last3", "First3", "Class1", "Period1", "GitHubUser")
				.AddProject("Class1", "Project1")
				.AddProjectTestClass("Class1", "Project1", "TestClass1")
				.AddProjectTestClass("Class1", "Project1", "TestClass2")
				.AddCheckpoint("Class1", "Project1", "Checkpoint1")
				.AddCheckpointDueDate("Class1", "Project1", "Checkpoint1", "Period1", DateTime.MinValue)
				.AddCommit("Class1", "Project1", "Student1", "Commit1", Commit1PushDate, GetBuildSomeTestsPass())
				.AddCommit("Class1", "Project1", "Student1", "Commit2", Commit2PushDate, GetFailedBuild())
				.AddCommit("Class1", "Project1", "Student1", "Commit3", Commit3PushDate, GetBuildAllTestsPass())
				.AddCommit("Class1", "Project1", "Student2", "Commit1", Commit1PushDate, GetFailedBuild())
				.AddCommit("Class1", "Project1", "Student3", "Commit1", Commit1PushDate, GetBuildSomeTestsPass())
				.AddCommit("Class1", "Project1", "Student3", "Commit2", Commit2PushDate, GetFailedBuild())
				.AddCommit("Class1", "Project1", "Student3", "Commit3", Commit3PushDate, null /*in progress */, "BuildJobId", "BuildRequestToken")
				.AddSubmission("Class1", "Project1", "Checkpoint1", "Student1", "Commit3", DateTime.MinValue);
		}

		/// <summary>
		/// Returns build 1 for use during testing.
		/// </summary>
		private Build GetBuildSomeTestsPass()
		{
			return new Build()
			{
				Status = BuildStatus.Completed,
				DateStarted = Commit1PushDate,
				DateCompleted = Commit1PushDate + SuccessfulBuildDuration,
				TestResults = Collections.CreateList
				(
					new TestResult()
					{
						ClassName = "TestClass1",
						TestName = "Test1",
						Succeeded = false,
						PreviouslySucceeded = false,
						FailureMessage = "Test1 Failure Message",
						FailureTrace = "Test1 Stack Trace"
					},
					new TestResult()
					{
						ClassName = "TestClass2",
						TestName = "Test2",
						Succeeded = true,
						PreviouslySucceeded = false
					}
				)
			};
		}

		/// <summary>
		/// Returns build 2 for use during testing.
		/// </summary>
		private Build GetFailedBuild()
		{
			return new Build()
			{
				Status = BuildStatus.Error,
				Output = "Build Error",
				DateStarted = Commit1PushDate,
				DateCompleted = Commit1PushDate + FailedBuildDuration
			};
		}

		/// <summary>
		/// Returns build 3 for use during testing.
		/// </summary>
		private Build GetBuildAllTestsPass()
		{
			return new Build()
			{
				Status = BuildStatus.Completed,
				DateStarted = Commit1PushDate,
				DateCompleted = Commit1PushDate + SuccessfulBuildDuration,
				TestResults = Collections.CreateList
				(
					new TestResult()
					{
						ClassName = "TestClass1",
						TestName = "Test1",
						Succeeded = true,
						PreviouslySucceeded = false
					},
					new TestResult()
					{
						ClassName = "TestClass2",
						TestName = "Test2",
						Succeeded = true,
						PreviouslySucceeded = true
					}
				)
			};
		}

		/// <summary>
		/// Returns a new build service.
		/// </summary>
		private IBuildService GetBuildService(
			DatabaseContext dbContext,
			IJobQueueClient jobQueueClient = null,
			ITimeProvider timeProvider = null)
		{
			return new Service.Projects.BuildService(dbContext, jobQueueClient, timeProvider);
		}

		/// <summary>
		/// Returns a mock monitoring API.
		/// </summary>
		private Mock<IJobQueueClient> GetMockJobQueueClient(
			JobState jobState,
			DateTime? enteredState = null)
		{
			var jobQueueClient = new Mock<IJobQueueClient>();

			jobQueueClient
				.Setup(client => client.GetJobStatusAsync("BuildJobId"))
				.ReturnsAsync
				(
					new JobStatus
					(
						jobState, 
						enteredState ?? DateTime.MinValue
					)
				);

			return jobQueueClient;
		}

		/// <summary>
		/// Returns a mock time provider.
		/// </summary>
		private Mock<ITimeProvider> GetMockTimeProvider(DateTime currentTime)
		{
			var timeProvider = new Mock<ITimeProvider>();
			timeProvider
				.Setup(tp => tp.UtcNow)
				.Returns(currentTime);

			return timeProvider;
		}

		/// <summary>
		/// The push date for commit 1.
		/// </summary>
		private static readonly DateTime Commit1PushDate = new DateTime(2016, 1, 1);

		/// <summary>
		/// The push date for commit 2.
		/// </summary>
		private static readonly DateTime Commit2PushDate = new DateTime(2016, 1, 2);

		/// <summary>
		/// The push date for commit 3.
		/// </summary>
		private static readonly DateTime Commit3PushDate = new DateTime(2016, 1, 3);

		/// <summary>
		/// The duration of a successful build.
		/// </summary>
		private static readonly TimeSpan SuccessfulBuildDuration = TimeSpan.FromSeconds(10);

		/// <summary>
		/// The duration of a failed build.
		/// </summary>
		private static readonly TimeSpan FailedBuildDuration = TimeSpan.FromSeconds(1);
	}
}
