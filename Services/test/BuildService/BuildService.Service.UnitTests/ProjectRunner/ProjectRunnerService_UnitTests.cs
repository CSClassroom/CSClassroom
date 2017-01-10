using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.BuildService.Model.CodeRunner;
using CSC.BuildService.Model.ProjectRunner;
using CSC.BuildService.Service.CodeRunner;
using CSC.BuildService.Service.Docker;
using CSC.BuildService.Service.ProjectRunner;
using CSC.BuildService.Service.UnitTests.TestDoubles;
using CSC.Common.Infrastructure.Serialization;
using CSC.Common.Infrastructure.System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CSC.BuildService.Service.UnitTests.ProjectRunner
{
	/// <summary>
	/// Unit tests for the ProjectRunnerService class.
	/// </summary>
	public class ProjectRunnerService_UnitTests
	{
		/// <summary>
		/// Ensures that the GitHub auth token is correctly sent
		/// to the project runner.
		/// </summary>
		[Fact]
		public async Task ExecuteProjectJobAsync_VerifyAuthEnvironmentVariable()
		{
			var dockerHost = GetMockDockerHost();
			var projectJob = GetProjectJob();
			var config = GetProjectRunnerConfig(oAuthToken: "OAuthToken");

			var projectRunnerService = GetProjectRunnerService(dockerHost, config);
			await projectRunnerService.ExecuteProjectJobAsync(projectJob, "OperationId");

			Assert.Equal("OAuthToken", dockerHost.EnvironmentVariables["GITHUB_OAUTH_TOKEN"]);
		}

		/// <summary>
		/// Ensures that the GitHub organization name is correctly sent
		/// to the project runner.
		/// </summary>
		[Fact]
		public async Task ExecuteProjectJobAsync_VerifyGitHubOrgEnvironmentVariable()
		{
			var dockerHost = GetMockDockerHost();
			var projectJob = GetProjectJob(gitHubOrg: "GitHubOrg");

			var projectRunnerService = GetProjectRunnerService(dockerHost);
			await projectRunnerService.ExecuteProjectJobAsync(projectJob, "OperationId");

			Assert.Equal("GitHubOrg", dockerHost.EnvironmentVariables["GITHUB_ORG_NAME"]);
		}

		/// <summary>
		/// Ensures that the GitHub submission repository name is correctly sent
		/// to the project runner.
		/// </summary>
		[Fact]
		public async Task ExecuteProjectJobAsync_VerifySubmissionRepoNameEnvironmentVariable()
		{
			var dockerHost = GetMockDockerHost();
			var projectJob = GetProjectJob(submissionRepo: "SubmissionRepo");

			var projectRunnerService = GetProjectRunnerService(dockerHost);
			await projectRunnerService.ExecuteProjectJobAsync(projectJob, "OperationId");

			Assert.Equal("SubmissionRepo", dockerHost.EnvironmentVariables["GITHUB_SUBMISSION_REPO_NAME"]);
		}

		/// <summary>
		/// Ensures that the GitHub template repository name is correctly sent
		/// to the project runner.
		/// </summary>
		[Fact]
		public async Task ExecuteProjectJobAsync_VerifyTemplateRepoNameEnvironmentVariable()
		{
			var dockerHost = GetMockDockerHost();
			var projectJob = GetProjectJob(templateRepo: "TemplateRepo");

			var projectRunnerService = GetProjectRunnerService(dockerHost);
			await projectRunnerService.ExecuteProjectJobAsync(projectJob, "OperationId");

			Assert.Equal("TemplateRepo", dockerHost.EnvironmentVariables["GITHUB_TEMPLATE_REPO_NAME"]);
		}

		/// <summary>
		/// Ensures that the project name is correctly sent to the project runner.
		/// </summary>
		[Fact]
		public async Task ExecuteProjectJobAsync_VerifyProjectNameEnvironmentVariable()
		{
			var dockerHost = GetMockDockerHost();
			var projectJob = GetProjectJob(projectName: "ProjectName");

			var projectRunnerService = GetProjectRunnerService(dockerHost);
			await projectRunnerService.ExecuteProjectJobAsync(projectJob, "OperationId");

			Assert.Equal("ProjectName", dockerHost.EnvironmentVariables["PROJECT_NAME"]);
		}

		/// <summary>
		/// Ensures that the commit SHA is correctly sent to the project runner.
		/// </summary>
		[Fact]
		public async Task ExecuteProjectJobAsync_VerifyCommitShaEnvironmentVariable()
		{
			var dockerHost = GetMockDockerHost();
			var projectJob = GetProjectJob(commitSha: "CommitSHA");

			var projectRunnerService = GetProjectRunnerService(dockerHost);
			await projectRunnerService.ExecuteProjectJobAsync(projectJob, "OperationId");

			Assert.Equal("CommitSHA", dockerHost.EnvironmentVariables["COMMIT_SHA"]);
		}

		/// <summary>
		/// Ensures that the paths to copy from template project to the submission project 
		/// are correctly sent to the project runner.
		/// </summary>
		[Fact]
		public async Task ExecuteProjectJobAsync_VerifyCopyPathsEnvironmentVariable()
		{
			var dockerHost = GetMockDockerHost();
			var projectJob = GetProjectJob(copyPaths: new List<string>() { "Path1", "Path2" });

			var projectRunnerService = GetProjectRunnerService(dockerHost);
			await projectRunnerService.ExecuteProjectJobAsync(projectJob, "OperationId");

			Assert.Equal("Path1;Path2", dockerHost.EnvironmentVariables["PATHS_TO_COPY"]);
		}

		/// <summary>
		/// Ensures that the test classes are correctly sent to the project runner.
		/// </summary>
		[Fact]
		public async Task ExecuteProjectJobAsync_VerifyTestClassesEnvironmentVariable()
		{
			var dockerHost = GetMockDockerHost();
			var projectJob = GetProjectJob(testClasses: new List<string>() {"Class1", "Class2"});

			var projectRunnerService = GetProjectRunnerService(dockerHost);
			await projectRunnerService.ExecuteProjectJobAsync(projectJob, "OperationId");

			Assert.Equal("Class1;Class2", dockerHost.EnvironmentVariables["TEST_CLASSES"]);
		}

		/// <summary>
		/// Ensures that the correct URL is notified of the result.
		/// </summary>
		[Fact]
		public async Task ExecuteProjectJobAsync_NotifiesCorrectUrl()
		{
			var dockerHost = GetMockDockerHost();
			var projectJob = GetProjectJob(callbackPath: "CallbackPath");
			var config = GetProjectRunnerConfig(callbackHost: "CallbackHost");
			var notifier = GetMockNotifier();

			var projectRunnerService = GetProjectRunnerService(dockerHost, config, notifier);
			await projectRunnerService.ExecuteProjectJobAsync(projectJob, "OperationId");

			Assert.Equal("CallbackHost", notifier.CallbackHost);
			Assert.Equal("CallbackPath", notifier.CallbackPath);
		}

		/// <summary>
		/// Ensures that the notification contains the correct operation ID.
		/// </summary>
		[Fact]
		public async Task ExecuteProjectJobAsync_NotifiesWithCorrectOperationId()
		{
			var dockerHost = GetMockDockerHost();
			var projectJob = GetProjectJob();
			var notifier = GetMockNotifier();

			var projectRunnerService = GetProjectRunnerService(dockerHost, notifier: notifier);
			await projectRunnerService.ExecuteProjectJobAsync(projectJob, "OperationId");

			Assert.Equal("OperationId", notifier.OperationId);
		}

		/// <summary>
		/// Ensures that the notification contains the correct build request token.
		/// </summary>
		[Fact]
		public async Task ExecuteProjectJobAsync_NotifiesWithCorrectBuildRequestToken()
		{
			var dockerHost = GetMockDockerHost();
			var projectJob = GetProjectJob(buildRequestToken: "BuildRequestToken");
			var notifier = GetMockNotifier();

			var projectRunnerService = GetProjectRunnerService(dockerHost, notifier: notifier);
			await projectRunnerService.ExecuteProjectJobAsync(projectJob, "OperationId");

			Assert.Equal("BuildRequestToken", notifier.Result.BuildRequestToken);
		}

		/// <summary>
		/// Ensures that the notification contains the correct start and end times
		/// for the build.
		/// </summary>
		[Fact]
		public async Task ExecuteProjectJobAsync_NotifiesWithCorrectBuildTiming()
		{
			var dockerHost = GetMockDockerHost();
			var projectJob = GetProjectJob();
			var notifier = GetMockNotifier();
			var timeProvider = GetMockTimeProvider
			(
				new DateTime(2016, 1, 1), 
				new DateTime(2016, 1, 2)
			);

			var projectRunnerService = GetProjectRunnerService
			(
				dockerHost, 
				notifier: notifier,
				timeProvider: timeProvider.Object
			);
			
			await projectRunnerService.ExecuteProjectJobAsync(projectJob, "OperationId");

			Assert.Equal(new DateTime(2016, 1, 1), notifier.Result.JobStartedDate);
			Assert.Equal(new DateTime(2016, 1, 2), notifier.Result.JobFinishedDate);
		}

		/// <summary>
		/// Ensures that the notification contains the build output.
		/// </summary>
		[Fact]
		public async Task ExecuteProjectJobAsync_NotifiesWithCorrectBuildOutput()
		{
			var projectJob = GetProjectJob();
			var config = GetProjectRunnerConfig();
			var notifier = GetMockNotifier();
			var dockerHost = GetMockDockerHost
			(
				new DockerResult
				(
					completed: true,
					output: "BuildOutput",
					response: null
				)
			);

			var projectRunnerService = GetProjectRunnerService(dockerHost, config, notifier);
			await projectRunnerService.ExecuteProjectJobAsync(projectJob, "OperationId");

			Assert.Equal("BuildOutput", notifier.Result.BuildOutput);
		}

		/// <summary>
		/// Ensures that the notifier is notified of a timeout result when 
		/// the project runner times out.
		/// </summary>
		[Fact]
		public async Task ExecuteProjectJobAsync_JobTimesOut_NotifiesWithTimeoutResult()
		{
			var projectJob = GetProjectJob();
			var notifier = GetMockNotifier();
			var dockerHost = GetMockDockerHost
			(
				new DockerResult
				(
					completed: false,
					output: "BuildOutput",
					response: null
				)
			);

			var projectRunnerService = GetProjectRunnerService
			(
				dockerHost, 
				notifier: notifier
			);

			await projectRunnerService.ExecuteProjectJobAsync(projectJob, "OperationId");

			Assert.False(notifier.Result.BuildSucceeded);
			Assert.Equal(ProjectJobStatus.Timeout, notifier.Result.Status);
		}

		/// <summary>
		/// Ensures that the notifier is correctly notified of a build failure.
		/// </summary>
		[Fact]
		public async Task ExecuteProjectJobAsync_BuildFailure_NotifiesWithErrorResult()
		{
			var projectJob = GetProjectJob();
			var notifier = GetMockNotifier();
			var dockerHost = GetMockDockerHost
			(
				new DockerResult
				(
					completed: true,
					output: "BuildOutput",
					response: null
				)
			);

			var projectRunnerService = GetProjectRunnerService
			(
				dockerHost,
				notifier: notifier
			);

			await projectRunnerService.ExecuteProjectJobAsync(projectJob, "OperationId");

			Assert.False(notifier.Result.BuildSucceeded);
			Assert.Equal(ProjectJobStatus.Error, notifier.Result.Status);
		}

		/// <summary>
		/// Ensures that the notifier is notified of a timeout when the test
		/// results can't be deserialized. While it isn't technically a timeout,
		/// a partially written response file usually means that java ran out of
		/// memory when running the tests. This typically is indicative of an
		/// infinite loop or a very long-running test.
		/// </summary>
		[Fact]
		public async Task ExecuteProjectJobAsync_InvalidTestResults_NotifiesWithTimeoutResult()
		{
			var projectJob = GetProjectJob();
			var notifier = GetMockNotifier();

			var serializer = GetMockJsonSerializer
			(
				"InvalidTestResults", 
				deserializedTestResults: null
			);

			var dockerHost = GetMockDockerHost
			(
				new DockerResult
				(
					completed: true,
					output: "BuildOutput",
					response: "InvalidTestResults"
				)
			);

			var projectRunnerService = GetProjectRunnerService
			(
				dockerHost,
				notifier: notifier,
				serializer: serializer.Object
			);

			await projectRunnerService.ExecuteProjectJobAsync(projectJob, "OperationId");

			Assert.False(notifier.Result.BuildSucceeded);
			Assert.Equal(ProjectJobStatus.Timeout, notifier.Result.Status);
		}

		/// <summary>
		/// Ensures that the notifier is correctly notified of a build failure.
		/// </summary>
		[Fact]
		public async Task ExecuteProjectJobAsync_BuildSucceeded_NotifiesWithTestResults()
		{
			var projectJob = GetProjectJob();
			var notifier = GetMockNotifier();
			var testResults = new List<TestResult>();

			var serializer = GetMockJsonSerializer
			(
				"SerializedTestResults", 
				testResults
			);

			var dockerHost = GetMockDockerHost
			(
				new DockerResult
				(
					completed: true,
					output: "BuildOutput",
					response: "SerializedTestResults"
				)
			);

			var projectRunnerService = GetProjectRunnerService
			(
				dockerHost,
				notifier: notifier,
				serializer: serializer.Object
			);

			await projectRunnerService.ExecuteProjectJobAsync(projectJob, "OperationId");

			Assert.True(notifier.Result.BuildSucceeded);
			Assert.Equal(ProjectJobStatus.Completed, notifier.Result.Status);
			Assert.Equal(testResults, notifier.Result.TestResults);
		}

		/// <summary>
		/// Returns a project job.
		/// </summary>
		private ProjectJob GetProjectJob(
			string buildRequestToken = null,
			string gitHubOrg = null,
			string projectName = null,
			string submissionRepo = null,
			string templateRepo = null,
			string commitSha = null,
			IList<string> copyPaths = null,
			IList<string> testClasses = null,
			string callbackPath = null)
		{
			return new ProjectJob
			(
				buildRequestToken,
				gitHubOrg,
				projectName,
				submissionRepo,
				templateRepo,
				commitSha,
				copyPaths ?? new List<string>(),
				testClasses ?? new List<string>(),
				callbackPath	
			);
		}

		/// <summary>
		/// Returns a configuration object for the project runner service.
		/// </summary>
		private IProjectRunnerServiceConfig GetProjectRunnerConfig(
			string oAuthToken = null,
			string callbackHost = null)
		{
			return new ProjectRunnerServiceConfig(oAuthToken, callbackHost);
		}

		/// <summary>
		/// Returns a mock docker host factory.
		/// </summary>
		private MockDockerHost GetMockDockerHost(
			DockerResult dockerResult = null)
		{
			return new MockDockerHost
			(
				dockerResult ?? new DockerResult
				(
					completed: true,
					output: "DiagnosticOutput",
					response: null
				)
			);
		}

		/// <summary>
		/// Returns a mock docker host factory.
		/// </summary>
		private Mock<IDockerHostFactory> GetMockDockerHostFactory(
			MockDockerHost dockerHost)
		{
			var dockerHostFactory = new Mock<IDockerHostFactory>();

			dockerHostFactory
				.Setup(factory => factory.CreateDockerHost("ProjectRunner"))
				.Returns(dockerHost);

			return dockerHostFactory;
		}

		/// <summary>
		/// Returns a mock Json serializer that does not expect to
		/// deserialize anything.
		/// </summary>
		private Mock<IJsonSerializer> GetMockJsonSerializer()
		{
			return new Mock<IJsonSerializer>();
		}

		/// <summary>
		/// Returns a mock Json serializer that expects to deserialize
		/// test results.
		/// </summary>
		private Mock<IJsonSerializer> GetMockJsonSerializer(
			string serializedTestResults,
			List<TestResult> deserializedTestResults)
		{
			var jsonSerializer = new Mock<IJsonSerializer>();

			var setup = jsonSerializer
				.Setup(s => s.Deserialize<List<TestResult>>(serializedTestResults));

			if (deserializedTestResults != null)
			{
				setup.Returns(deserializedTestResults);
			}
			else
			{
				setup.Throws<InvalidOperationException>();
			}

			return jsonSerializer;
		}

		/// <summary>
		/// Returns a mock project job result notifier.
		/// </summary>
		private MockProjectJobResultNotifier GetMockNotifier()
		{
			return new MockProjectJobResultNotifier();
		}

		/// <summary>
		/// Returns a mock time provider.
		/// </summary>
		private Mock<ITimeProvider> GetMockTimeProvider(params DateTime[] times)
		{
			var timeProvider = new Mock<ITimeProvider>();

			if (times.Length == 0)
			{
				times = new DateTime[2];
			}

			timeProvider
				.Setup(tp => tp.UtcNow)
				.Returns(new Queue<DateTime>(times).Dequeue);

			return timeProvider;
		}

		/// <summary>
		/// Returns an instance of the project runner service.
		/// </summary>
		private IProjectRunnerService GetProjectRunnerService(
			MockDockerHost dockerHost,
			IProjectRunnerServiceConfig config = null,
			IProjectJobResultNotifier notifier = null,
			IJsonSerializer serializer = null,
			ITimeProvider timeProvider = null)
		{
			return new ProjectRunnerService
			(
				new Mock<ILogger<ProjectRunnerService>>().Object,
				config ?? GetProjectRunnerConfig(),
				GetMockDockerHostFactory(dockerHost).Object,
				serializer ?? GetMockJsonSerializer().Object,
				notifier ?? GetMockNotifier(),
				timeProvider ?? GetMockTimeProvider().Object
			);
		}
	}
}