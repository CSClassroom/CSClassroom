using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSC.BuildService.Model.ProjectRunner;
using CSC.BuildService.Service.ProjectRunner;
using CSC.Common.Infrastructure.GitHub;
using CSC.Common.Infrastructure.Logging;
using CSC.Common.Infrastructure.Queue;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Projects.PushEvents;
using CSC.CSClassroom.Service.Projects.Repositories;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Projects.PushEvents
{
	/// <summary>
	/// Unit tests for the PushEventProcessor class.
	/// </summary>
	public class PushEventProcessor_UnitTests
	{
		/// <summary>
		/// Ensures that GetNewCommitsToProcess only returns new commits.
		/// </summary>
		[Fact]
		public void GetNewCommitsToProcess_ReturnsOnlyNewCommits()
		{
			var project = GetProject();
			var studentPushEvents = GetPushEvents(userId: 2);
			var existingCommits = Collections.CreateList
			(
				new CommitDescriptor
				(
					sha: "PreviousCommitSha", 
					projectId: 1, 
					userId: 2
				)
			);

			var pushEventProcessor = GetPushEventProcessor();

			var results = pushEventProcessor.GetNewCommitsToProcess
			(
				project,
				existingCommits,
				studentPushEvents
			);

			Assert.Equal(1, results.Count);
			Assert.Equal(studentPushEvents[0].PushEvents[0], results[0].PushEvent);
			Assert.Equal(1, results[0].Commit.ProjectId);
			Assert.Equal(CommitDate.UtcDateTime, results[0].Commit.CommitDate);
			Assert.Equal(PushDate.UtcDateTime, results[0].Commit.PushDate);
			Assert.Equal("NewCommitSha", results[0].Commit.Sha);
			Assert.Equal("NewCommitMessage", results[0].Commit.Message);
			Assert.Equal(2, results[0].Commit.UserId);
			Assert.NotNull(results[0].Commit.BuildRequestToken);
		}

		/// <summary>
		/// Ensures that CreateBuildJobAsync actually creates the correct 
		/// build job for the given commit.
		/// </summary>
		[Fact]
		public async Task CreateBuildJobAsync_CreatesCorrectJob()
		{
			var project = GetProject();
			var studentPushEvents = GetPushEvents(userId: 2);
			var pushEventCommit = new Service.Projects.PushEvents.PushEventCommit
			(
				studentPushEvents[0].PushEvents[0],
				new Commit()
				{
					ProjectId = 1,
					BuildRequestToken = "BuildRequestToken",
					Sha = "NewCommitSha"
				}
			);

			var operationIdProvider = GetMockOperationIdProvider();
			var projectRunnerService = GetMockProjectRunnerService();
			var jobQueueClient = GetMockJobQueueClient(projectRunnerService.Object);

			var pushEventProcessor = GetPushEventProcessor
			(
				jobQueueClient: jobQueueClient.Object,
				operationIdProvider: operationIdProvider.Object
			);

			var result = await pushEventProcessor.CreateBuildJobAsync
			(
				project,
				pushEventCommit,
				"BuildResultCallbackUrl"
			);

			Assert.Equal("BuildJobId", result);
			projectRunnerService.Verify(GetExecuteProjectJobExpression(), Times.Once);
		}

		/// <summary>
		/// Returns a mock project runner service.
		/// </summary>
		private static Mock<IProjectRunnerService> GetMockProjectRunnerService()
		{
			var projectRunnerService = new Mock<IProjectRunnerService>();
			projectRunnerService
				.Setup(GetExecuteProjectJobExpression())
				.Returns(Task.CompletedTask);

			return projectRunnerService;
		}

		/// <summary>
		/// Returns a mock background job client, which executes the given job immediately.
		/// </summary>
		private static Mock<IJobQueueClient> GetMockJobQueueClient(
			IProjectRunnerService projectRunnerService)
		{
			var jobQueueClient = new Mock<IJobQueueClient>();
			jobQueueClient
				.Setup
				(
					jqc => jqc.EnqueueAsync<IProjectRunnerService>
					(
						It.IsAny<Expression<Func<IProjectRunnerService, Task>>>()
					)
				)
				.Callback<Expression<Func<IProjectRunnerService, Task>>>
				(
					jobExpression => jobExpression.Compile()(projectRunnerService)
				)
				.ReturnsAsync("BuildJobId");

			return jobQueueClient;
		}

		/// <summary>
		/// Returns the expected expression for executing a project job.
		/// </summary>
		private static Expression<Func<IProjectRunnerService, Task>> GetExecuteProjectJobExpression()
		{
			return prs => prs.ExecuteProjectJobAsync
			(
				It.Is<ProjectJob>
				(
					job => job.ProjectName == "Project1"
						&& job.TemplateRepo == "Project1_Template"
						&& job.SubmissionRepo == "Project1_LastNameFirstName"
						&& job.GitHubOrg == "GitHubOrg"
						&& job.CommitSha == "NewCommitSha"
						&& job.CallbackPath == "BuildResultCallbackUrl"
						&& job.BuildRequestToken == "BuildRequestToken"
						&& job.CopyPaths.SequenceEqual
						(
							Collections.CreateList
							(
								"Private1",
								"Private2",
								"Immutable1",
								"Immutable2"
							)
						)
						&& job.TestClasses.SequenceEqual
						(
							Collections.CreateList
							(
								"TestClass1",
								"TestClass2"	
							)
						)
				),
				"OperationId"
			);
		}

		/// <summary>
		/// Returns a mock operation ID provider.
		/// </summary>
		private static Mock<IOperationIdProvider> GetMockOperationIdProvider()
		{
			var operationIdProvider = new Mock<IOperationIdProvider>();
			operationIdProvider
				.Setup(op => op.OperationId)
				.Returns("OperationId");
			return operationIdProvider;
		}

		/// <summary>
		/// Returns a new push event processor.
		/// </summary>
		private PushEventProcessor GetPushEventProcessor(
			IJobQueueClient jobQueueClient = null,
			IOperationIdProvider operationIdProvider = null)
		{
			return new PushEventProcessor(jobQueueClient, operationIdProvider);
		}

		/// <summary>
		/// Returns a project.
		/// </summary>
		private Project GetProject()
		{
			return new Project()
			{
				Id = 1,
				Name = "Project1",
				ExplicitSubmissionRequired = true,
				TestClasses = Collections.CreateList
				(
					new TestClass() { ClassName = "TestClass1" },
					new TestClass() { ClassName = "TestClass2" }	
				),
				ImmutableFilePaths = Collections.CreateList
				(
					new ImmutableFilePath() { Path = "Immutable1" },
					new ImmutableFilePath() { Path = "Immutable2" }
				),
				PrivateFilePaths = Collections.CreateList
				(
					new PrivateFilePath() { Path = "Private1" },
					new PrivateFilePath() { Path = "Private2" }
				)
			};
		}

		/// <summary>
		/// Returns a set of push events.
		/// </summary>
		private IList<StudentRepoPushEvents> GetPushEvents(int userId)
		{
			return Collections.CreateList
			(
				new StudentRepoPushEvents
				(
					new ClassroomMembership() { UserId = userId },
					Collections.CreateList
					(
						new GitHubPushEvent()
						{
							CreatedAt = PushDate,
							Repository = new PushEventRepository()
							{
								Owner = new PushEventRepositoryOwner() { Name = "GitHubOrg" },
								Name = "Project1_LastNameFirstName"
							},
							Commits = Collections.CreateList
							(
								new GitHubPushEventCommit()
								{
									Id = "PreviousCommitSha",
									Message = "PreviousCommitMessage",
									Timestamp = CommitDate - TimeSpan.FromHours(1)
								},
								new GitHubPushEventCommit()
								{
									Id = "NewCommitSha",
									Message = "NewCommitMessage",
									Timestamp = CommitDate
								}
							)
						}
					)
				)
			);
		}

		/// <summary>
		/// An example commit date.
		/// </summary>
		private static readonly DateTimeOffset CommitDate = new DateTimeOffset
		(
			new DateTime(2016, 1, 1),
			TimeSpan.FromHours(1)
		);

		/// <summary>
		/// An example push date.
		/// </summary>
		private static readonly DateTimeOffset PushDate = new DateTimeOffset
		(
			new DateTime(2016, 1, 2),
			TimeSpan.FromHours(1)
		);
	}
}
