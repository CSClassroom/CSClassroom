using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.BuildService.Model.ProjectRunner;
using CSC.BuildService.Service.Docker;
using CSC.Common.Infrastructure.Serialization;
using CSC.Common.Infrastructure.System;
using Microsoft.Extensions.Logging;

namespace CSC.BuildService.Service.ProjectRunner
{
	/// <summary>
	/// The project runner service.
	/// </summary>
	public class ProjectRunnerService : IProjectRunnerService
	{
		/// <summary>
		/// A logger.
		/// </summary>
		private readonly ILogger _logger;

		/// <summary>
		/// The configuration of the service.
		/// </summary>
		private readonly IProjectRunnerServiceConfig _config;

		/// <summary>
		/// The docker host factory.
		/// </summary>
		private readonly IDockerHostFactory _dockerHostFactory;

		/// <summary>
		/// The JSON serializer.
		/// </summary>
		private readonly IJsonSerializer _jsonSerializer;

		/// <summary>
		/// The project job source notifier.
		/// </summary>
		private readonly IProjectJobResultNotifier _notifier;

		/// <summary>
		/// The time provider.
		/// </summary>
		private readonly ITimeProvider _timeProvider;

		/// <summary>
		/// The name of the image to run.
		/// </summary>
		private const string c_projectRunnerId = "ProjectRunner";

		/// <summary>
		/// Environment variable strings
		/// </summary>
		private const string c_githubOAuthTokenVar = "GITHUB_OAUTH_TOKEN";
		private const string c_githubOrgNameVar = "GITHUB_ORG_NAME";
		private const string c_githubSubmissionRepoNameVar = "GITHUB_SUBMISSION_REPO_NAME";
		private const string c_githubTemplateRepoNameVar = "GITHUB_TEMPLATE_REPO_NAME";
		private const string c_projectNameVar = "PROJECT_NAME";
		private const string c_commitShaVar = "COMMIT_SHA";
		private const string c_testClassesVar = "TEST_CLASSES";
		private const string c_pathsToCopyVar = "PATHS_TO_COPY";

		/// <summary>
		/// Constructor.
		/// </summary>
		public ProjectRunnerService(
			ILogger<ProjectRunnerService> logger,
			IProjectRunnerServiceConfig config,
			IDockerHostFactory dockerHostFactory,
			IJsonSerializer jsonSerializer,
			IProjectJobResultNotifier notifier,
			ITimeProvider timeProvider)
		{
			_logger = logger;
			_config = config;
			_dockerHostFactory = dockerHostFactory;
			_jsonSerializer = jsonSerializer;
			_notifier = notifier;
			_timeProvider = timeProvider;
		}

		/// <summary>
		/// Executes a project job, notifying the callback path when complete.
		/// </summary>
		public async Task ExecuteProjectJobAsync(ProjectJob projectJob, string operationId)
		{
			using (_logger.BeginScope(
				new Dictionary<string, object>()
				{
					["OperationId"] = operationId,
					["GitHubOrg"] = projectJob.GitHubOrg,
					["GitHubRepo"] = projectJob.SubmissionRepo,
					["CommitSha"] = projectJob.CommitSha
				}))
			{
				_logger.LogInformation("Starting project job.");

				var result = await RunJobAsync(projectJob, operationId);

				_logger.LogInformation(
					"Project job completed with {status} status and {numTestResults} test results.",
					result.Status.ToString() ?? "Unknown",
					result.TestResults?.Count ?? 0);
				
				await _notifier.NotifyAsync
				(
					_config.ProjectJobResultHost,
					projectJob.CallbackPath,
					operationId,
					result
				);

				_logger.LogInformation("Sent notification for job completion.");
			}
		}

		/// <summary>
		/// Runs the job, and returns the result.
		/// </summary>
		private async Task<ProjectJobResult> RunJobAsync(ProjectJob projectJob, string operationId)
		{
			var dockerHost = _dockerHostFactory.CreateDockerHost(c_projectRunnerId);

			var jobStarted = _timeProvider.UtcNow;

			var dockerResult = await dockerHost.RunImageInNewContainerAsync(
				requestContents: null,
				environmentVariables: new Dictionary<string, string>()
				{
					[c_githubOAuthTokenVar] = _config.GitHubOAuthToken,
					[c_githubOrgNameVar] = projectJob.GitHubOrg,
					[c_projectNameVar] = projectJob.ProjectName,
					[c_githubSubmissionRepoNameVar] = projectJob.SubmissionRepo,
					[c_githubTemplateRepoNameVar] = projectJob.TemplateRepo,
					[c_commitShaVar] = projectJob.CommitSha,
					[c_pathsToCopyVar] = string.Join(";", projectJob.CopyPaths),
					[c_testClassesVar] = string.Join(";", projectJob.TestClasses)
				});

			var jobFinished = _timeProvider.UtcNow;

			List<TestResult> testResults = null;
			bool validResponse = dockerResult.Completed
				&& dockerResult.Response != null
				&& TryDeserializeTestResults(dockerResult.Response, out testResults);

			return new ProjectJobResult()
			{
				BuildRequestToken = projectJob.BuildRequestToken,
				Status = GetProjectJobStatus
				(
					dockerResult.Completed, 
					dockerResult.Response != null, 
					validResponse
				),
				JobStartedDate = jobStarted,
				JobFinishedDate = jobFinished,
				BuildOutput = dockerResult.Output,
				TestResults = testResults
			};
		}

		/// <summary>
		/// Returns the project job status.
		/// </summary>
		private static ProjectJobStatus GetProjectJobStatus(
			bool jobComplete,
			bool hasResponse,
			bool validResponse)
		{
			return jobComplete
				? hasResponse
					? validResponse
						? ProjectJobStatus.Completed
						: ProjectJobStatus.Timeout
					: ProjectJobStatus.Error
				: ProjectJobStatus.Timeout;
		}

		/// <summary>
		/// Attempts to deserialize test results.
		/// </summary>
		private bool TryDeserializeTestResults(string response, out List<TestResult> testResults)
		{
			try
			{
				testResults = _jsonSerializer.Deserialize<List<TestResult>>(response);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(0, ex, "Failed to parse response: {response}", response);
				testResults = null;
				return false;
			}
		}
	}
}
