using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.GitHub;
using CSC.Common.Infrastructure.Serialization;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Repository;
using CSC.CSClassroom.Service.Projects;
using CSC.CSClassroom.Service.Projects.PushEvents;
using CSC.CSClassroom.Service.Projects.Repositories;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Projects
{
	/// <summary>
	/// Unit tests for the project service.
	/// </summary>
	public class ProjectService_UnitTests
	{
		/// <summary>
		/// Ensures that GetProjectsAsync returns only projects
		/// for a given classroom.
		/// </summary>
		[Fact]
		public async Task GetProjectsAsync_OnlyForClassroom()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddClassroom("Class2")
				.AddProject("Class1", "Project1")
				.AddProject("Class1", "Project2")
				.AddProject("Class2", "Project3")
				.Build();

			var projectService = GetProjectService(database.Context);
			var projects = await projectService.GetProjectsAsync("Class1");

			Assert.Equal(2, projects.Count);
			Assert.Equal("Project1", projects[0].Name);
			Assert.Equal("Project2", projects[1].Name);
		}

		/// <summary>
		/// Ensures that GetProjectAsync returns the desired
		/// project, if it exists.
		/// </summary>
		[Fact]
		public async Task GetProjectAsync_Exists_ReturnProject()
		{
			var database = GetDatabaseBuilderWithProject().Build();

			var projectService = GetProjectService(database.Context);
			var project = await projectService.GetProjectAsync("Class1", "Project1");

			Assert.Equal("Class1", project.Classroom.Name);
			Assert.Equal("Project1", project.Name);
		}

		/// <summary>
		/// Ensures that GetProjectAsync returns null, if the desired
		/// project doesn't exist.
		/// </summary>
		[Fact]
		public async Task GetProjectAsync_DoesntExist_ReturnNull()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.Build();

			var projectService = GetProjectService(database.Context);
			var project = await projectService.GetProjectAsync("Class1", "Project1");

			Assert.Null(project);
		}

		/// <summary>
		/// Ensures that CreateProjectAsync actually creates the project.
		/// </summary>
		[Fact]
		public async Task CreateProjectAsync_ProjectCreated()
		{
			var database = new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.Build();

			var projectService = GetProjectService(database.Context);
			await projectService.CreateProjectAsync
			(
				"Class1",
				new Project()
				{
					Name = "Project1",
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
				}
			);

			database.Reload();

			var project = database.Context.Projects
				.Include(p => p.Classroom)
				.Include(p => p.TestClasses)
				.Include(p => p.ImmutableFilePaths)
				.Include(p => p.PrivateFilePaths)
				.Single();

			Assert.Equal("Class1", project.Classroom.Name);
			Assert.Equal("Project1", project.Name);

			Assert.Equal(2, project.TestClasses.Count);
			Assert.Equal("TestClass1", project.TestClasses[0].ClassName);
			Assert.Equal("TestClass2", project.TestClasses[1].ClassName);

			Assert.Equal(2, project.ImmutableFilePaths.Count);
			Assert.Equal("Immutable1", project.ImmutableFilePaths[0].Path);
			Assert.Equal("Immutable2", project.ImmutableFilePaths[1].Path);

			Assert.Equal(2, project.PrivateFilePaths.Count);
			Assert.Equal("Private1", project.PrivateFilePaths[0].Path);
			Assert.Equal("Private2", project.PrivateFilePaths[1].Path);
		}

		/// <summary>
		/// Ensures that UpdateProjectAsync actually updates the project.
		/// </summary>
		[Fact]
		public async Task UpdateProjectAsync_ProjectUpdated()
		{
			var database = GetDatabaseBuilderWithProject().Build();

			var project = database.Context.Projects
				.Include(p => p.Classroom)
				.Include(p => p.TestClasses)
				.Include(p => p.ImmutableFilePaths)
				.Include(p => p.PrivateFilePaths)
				.Single();

			database.Reload();

			// Update the project
			project.TestClasses.RemoveAt(0);
			project.ImmutableFilePaths.RemoveAt(0);
			project.PrivateFilePaths.RemoveAt(0);
			project.TestClasses.Add(new TestClass() { ClassName = "TestClass3" });

			// Apply the update
			var projectService = GetProjectService(database.Context);
			await projectService.UpdateProjectAsync("Class1", project);

			database.Reload();

			project = database.Context.Projects
				.Include(qc => qc.Classroom)
				.Include(p => p.TestClasses)
				.Include(p => p.ImmutableFilePaths)
				.Include(p => p.PrivateFilePaths)
				.Single();

			var testClasses = project.TestClasses
				.OrderBy(tc => tc.ClassName)
				.ToList();

			Assert.Equal("Class1", project.Classroom.Name);
			Assert.Equal("Project1", project.Name);

			Assert.Equal(2, testClasses.Count);
			Assert.Equal("TestClass2", testClasses[0].ClassName);
			Assert.Equal(0, testClasses[0].Order);
			Assert.Equal("TestClass3", testClasses[1].ClassName);
			Assert.Equal(1, testClasses[1].Order);

			Assert.Single(project.ImmutableFilePaths);
			Assert.Equal("Immutable2", project.ImmutableFilePaths[0].Path);

			Assert.Single(project.PrivateFilePaths);
			Assert.Equal("Private2", project.PrivateFilePaths[0].Path);
		}

		/// <summary>
		/// Ensures that DeleteProjectAsync actually deletes a project.
		/// </summary>
		[Fact]
		public async Task DeleteProjectAsync_ProjectDeleted()
		{
			var database = GetDatabaseBuilderWithProject().Build();

			var projectService = GetProjectService(database.Context);
			await projectService.DeleteProjectAsync("Class1", "Project1");

			database.Reload();

			Assert.Equal(0, database.Context.Projects.Count());
		}

		/// <summary>
		/// Ensures GetTemplateFileListAsync returns the file list for a project.
		/// </summary>
		[Fact]
		public async Task GetTemplateFileListAsync_ReturnsResult()
		{
			var database = GetDatabaseBuilderWithProject().Build();

			var expectedResult = new List<ProjectRepositoryFile>();
			var repoPopulator = GetMockRepositoryPopulator(expectedResult);

			var projectService = GetProjectService
			(
				database.Context,
				repoPopulator: repoPopulator.Object
			);

			var result = await projectService.GetTemplateFileListAsync
			(
				"Class1",
				"Project1"
			);

			Assert.Equal(expectedResult, result);
		}

		/// <summary>
		/// Ensures CreateStudentRepositoriesAsync populates a student repository.
		/// </summary>
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task CreateStudentRepositoriesAsync_PopulatesRepositories(bool overwriteIfSafe)
		{
			var database = GetDatabaseBuilderWithProjectAndStudent().Build();

			var expectedResult = new List<CreateStudentRepoResult>();
			var repoPopulator = GetMockRepositoryPopulator
			(
				createResults: expectedResult,
				overwriteIfSafe: overwriteIfSafe
			);

			var projectService = GetProjectService
			(
				database.Context,
				repoPopulator: repoPopulator.Object
			);

			var result = await projectService.CreateStudentRepositoriesAsync
			(
				"Class1",
				"Project1",
				"Section1",
				"WebhookUrl",
				overwriteIfSafe: overwriteIfSafe
			);

			Assert.Equal(expectedResult, result);
		}

		/// <summary>
		/// Ensures VerifyGitHubWebhookPayloadSigned returns the correct result.
		/// </summary>
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void VerifyGitHubWebhookPayloadSigned_ReturnsResult(bool expectedResult)
		{
			var payload = new byte[0];
			var signature = "Signature";

			var webhookValidator = new Mock<IGitHubWebhookValidator>();
			webhookValidator
				.Setup(gwv => gwv.VerifyWebhookPayloadSigned(payload, signature))
				.Returns(expectedResult);

			var projectService = GetProjectService
			(
				dbContext: null,
				webhookValidator: webhookValidator.Object
			);

			var result = projectService.VerifyGitHubWebhookPayloadSigned(payload, signature);

			Assert.Equal(expectedResult, result);
		}

		/// <summary>
		/// Ensures that OnRepositoryPushAsync creates and stores the commits
		/// that were pushed.
		/// </summary>
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task OnRepositoryPushAsync_ValidPush_CommitCreated(
			bool buildableProject)
		{
			var database = GetDatabaseBuilderWithProjectAndStudent(buildableProject)
				.Build();

			var pushEvent = GetPushEvent("refs/heads/master");
			var pushEventProcessor = GetMockPushEventProcessor(pushEvent);
			var jsonSerializer = GetMockJsonSerializer(pushEvent);
			var timeProvider = GetMockTimeProvider(new DateTime(2016, 1, 1));

			var projectService = GetProjectService
			(
				database.Context,
				pushEventProcessor: pushEventProcessor.Object,
				jsonSerializer: jsonSerializer.Object,
				timeProvider: timeProvider.Object
			);

			await projectService.OnRepositoryPushAsync
			(
				"Class1",
				"SerializedPushEvent",
				"BuildResultCallbackUrl"
			);
			
			database.Reload();

			var newCommit = database.Context.Commits
				.SingleOrDefault(commit => commit.Sha == "NewCommitSha");

			Assert.NotNull(newCommit);
			Assert.Equal(new DateTime(2016, 1, 1), newCommit.PushDate);
		}

		/// <summary>
		/// Ensures that OnRepositoryPushAsync creates build jobs for the 
		/// commits that were pushed.
		/// </summary>
		[Fact]
		public async Task OnRepositoryPushAsync_ValidPushBuildableProject_BuildJobCreated()
		{
			var database = GetDatabaseBuilderWithProjectAndStudent(true /*buildableProject*/)
				.Build();

			var pushEvent = GetPushEvent("refs/heads/master");
			var pushEventProcessor = GetMockPushEventProcessor(pushEvent);
			var jsonSerializer = GetMockJsonSerializer(pushEvent);
			var timeProvider = GetMockTimeProvider(new DateTime(2016, 1, 1));

			var projectService = GetProjectService
			(
				database.Context,
				pushEventProcessor: pushEventProcessor.Object,
				jsonSerializer: jsonSerializer.Object,
				timeProvider: timeProvider.Object
			);

			await projectService.OnRepositoryPushAsync
			(
				"Class1",
				"SerializedPushEvent",
				"BuildResultCallbackUrl"
			);

			pushEventProcessor.Verify(GetCreateBuildJobExpression(pushEvent), Times.Once);
		}

		/// <summary>
		/// Ensures that OnRepositoryPushAsync creates build jobs for the 
		/// commits that were pushed.
		/// </summary>
		[Fact]
		public async Task OnRepositoryPushAsync_ValidPushNonBuildableProject_BuildJobNotCreated()
		{
			var database = GetDatabaseBuilderWithProjectAndStudent(false /*buildableProject*/)
				.Build();

			var pushEvent = GetPushEvent("refs/heads/master");
			var pushEventProcessor = GetMockPushEventProcessor(pushEvent);
			var jsonSerializer = GetMockJsonSerializer(pushEvent);
			var timeProvider = GetMockTimeProvider(new DateTime(2016, 1, 1));

			var projectService = GetProjectService
			(
				database.Context,
				pushEventProcessor: pushEventProcessor.Object,
				jsonSerializer: jsonSerializer.Object,
				timeProvider: timeProvider.Object
			);

			await projectService.OnRepositoryPushAsync
			(
				"Class1",
				"SerializedPushEvent",
				"BuildResultCallbackUrl"
			);

			pushEventProcessor.Verify(GetCreateBuildJobExpression(pushEvent), Times.Never);
		}

		/// <summary>
		/// Ensures that OnRepositoryPushAsync creates build jobs for
		/// the commits that were pushed.
		/// </summary>
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task OnRepositoryPushAsync_PushToNonMasterBranch_NoCommitCreated(
			bool buildableProject)
		{
			var database = GetDatabaseBuilderWithProjectAndStudent(buildableProject)
				.Build();

			var pushEvent = GetPushEvent("some-non-master-branch");
			var pushEventProcessor = GetMockPushEventProcessor(pushEvent);
			var jsonSerializer = GetMockJsonSerializer(pushEvent);
			var timeProvider = GetMockTimeProvider(new DateTime(2016, 1, 1));

			var projectService = GetProjectService
			(
				database.Context,
				pushEventProcessor: pushEventProcessor.Object,
				jsonSerializer: jsonSerializer.Object,
				timeProvider: timeProvider.Object
			);

			await projectService.OnRepositoryPushAsync
			(
				"Class1",
				"SerializedPushEvent",
				"BuildResultCallbackUrl"
			);

			database.Reload();

			var newCommit = database.Context.Commits
				.SingleOrDefault(commit => commit.Sha == "NewCommitSha");

			Assert.Null(newCommit);
		}

		/// <summary>
		/// Ensures that ProcessMissedCommitsForAllStudentsAsync creates a commit 
		/// for a missing push event.
		/// </summary>
		[Fact]
		public async Task ProcessMissedCommitsForAllStudentsAsync_CommitAdded()
		{
			var database = GetDatabaseBuilderWithProjectAndStudent().Build();

			var pushEvent = GetPushEvent("refs/heads/master");
			var pushEventRetriever = GetMockPushEventRetriever(pushEvent);
			var pushEventProcessor = GetMockPushEventProcessor(pushEvent);

			var projectService = GetProjectService
			(
				database.Context,
				pushEventRetriever: pushEventRetriever.Object,
				pushEventProcessor: pushEventProcessor.Object
			);

			await projectService.ProcessMissedCommitsForAllStudentsAsync
			(
				"Class1",
				"Project1",
				"BuildResultCallbackUrl"
			);

			database.Reload();

			var newCommit = database.Context.Commits
				.SingleOrDefault(commit => commit.Sha == "NewCommitSha");

			Assert.NotNull(newCommit);
		}

		/// <summary>
		/// Ensures that ProcessMissedCommitsForAllStudentsAsync creates a build 
		/// job for a missing push event.
		/// </summary>
		[Fact]
		public async Task ProcessMissedCommitsForAllStudentsAsync_BuildJobCreated()
		{
			var database = GetDatabaseBuilderWithProjectAndStudent().Build();

			var pushEvent = GetPushEvent("refs/heads/master");
			var pushEventRetriever = GetMockPushEventRetriever(pushEvent);
			var pushEventProcessor = GetMockPushEventProcessor(pushEvent);

			var projectService = GetProjectService
			(
				database.Context,
				pushEventRetriever: pushEventRetriever.Object,
				pushEventProcessor: pushEventProcessor.Object
			);

			await projectService.ProcessMissedCommitsForAllStudentsAsync
			(
				"Class1",
				"Project1",
				"BuildResultCallbackUrl"
			);

			pushEventProcessor.Verify(GetCreateBuildJobExpression(pushEvent), Times.Once);
		}

		/// <summary>
		/// Ensures that ProcessMissedPushEventsAsync creates a commit for a missing
		/// push event.
		/// </summary>
		[Fact]
		public async Task ProcessMissedCommitsForStudentAsync_CommitAdded()
		{
			var database = GetDatabaseBuilderWithProjectAndStudent().Build();

			var userId = database.Context.Users.First().Id;
			database.Reload();

			var pushEvent = GetPushEvent("refs/heads/master");
			var pushEventRetriever = GetMockPushEventRetriever(pushEvent);
			var pushEventProcessor = GetMockPushEventProcessor(pushEvent);

			var projectService = GetProjectService
			(
				database.Context,
				pushEventRetriever: pushEventRetriever.Object,
				pushEventProcessor: pushEventProcessor.Object
			);

			await projectService.ProcessMissedCommitsForStudentAsync
			(
				"Class1",
				"Project1",
				userId,
				"BuildResultCallbackUrl"
			);

			database.Reload();

			var newCommit = database.Context.Commits
				.SingleOrDefault(commit => commit.Sha == "NewCommitSha");

			Assert.NotNull(newCommit);
		}

		/// <summary>
		/// Ensures that ProcessMissedPushEventsAsync creates a build job for a missing
		/// push event.
		/// </summary>
		[Fact]
		public async Task ProcessMissedCommitsForStudentAsync_BuildJobCreated()
		{
			var database = GetDatabaseBuilderWithProjectAndStudent().Build();

			var userId = database.Context.Users.First().Id;
			database.Reload();

			var pushEvent = GetPushEvent("refs/heads/master");
			var pushEventRetriever = GetMockPushEventRetriever(pushEvent);
			var pushEventProcessor = GetMockPushEventProcessor(pushEvent);

			var projectService = GetProjectService
			(
				database.Context,
				pushEventRetriever: pushEventRetriever.Object,
				pushEventProcessor: pushEventProcessor.Object
			);

			await projectService.ProcessMissedCommitsForStudentAsync
			(
				"Class1",
				"Project1",
				userId,
				"BuildResultCallbackUrl"
			);

			pushEventProcessor.Verify(GetCreateBuildJobExpression(pushEvent), Times.Once);
		}
		
		/// <summary>
		/// Ensures that GetProjectStatusAsync returns the status of all projects
		/// for a given student, when there are successful builds.
		/// </summary>
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task GetProjectStatusAsync_SuccessfulBuilds_CorrectProjectStatus(
			bool lastBuildSucceeded)
		{
			var firstBuild = lastBuildSucceeded ? GetFailedBuild() : GetSuccessfulBuild();
			var secondBuild = lastBuildSucceeded ? GetSuccessfulBuild() : GetFailedBuild();
			var database = GetDatabaseBuilderWithProjectAndStudent
			(
				true /*buildable*/,
				firstBuild, 
				secondBuild
			).Build();

			var userId = database.Context.Users.First().Id;
			database.Reload();

			var projectService = GetProjectService(database.Context);
			var results = await projectService.GetProjectStatusAsync
			(
				"Class1",
				userId
			);

			Assert.Equal("LastName", results.LastName);
			Assert.Equal("FirstName", results.FirstName);
			Assert.Equal(userId, results.UserId);
			Assert.Equal(1, results.ProjectStatus.Count);
			Assert.Equal("Project1_LastNameFirstName", results.ProjectStatus[0].RepoName);
			Assert.Equal("Project1", results.ProjectStatus[0].ProjectName);
			Assert.Equal(lastBuildSucceeded, results.ProjectStatus[0].LastBuildSucceeded);
			Assert.Equal(SecondPushDate, results.ProjectStatus[0].LastCommitDate);
			Assert.Equal(1, results.ProjectStatus[0].BuildTestCounts.Count);
			Assert.Equal(1, results.ProjectStatus[0].BuildTestCounts[0].FailedCount);
			Assert.Equal(1, results.ProjectStatus[0].BuildTestCounts[0].PassedCount);
		}

		/// <summary>
		/// Ensures that GetProjectStatusAsync returns the status of all projects
		/// for a given student, when there is no successful build.
		/// </summary>
		[Fact]
		public async Task GetProjectStatusAsync_NoSuccessfulBuilds_CorrectProjectStatus()
		{
			var database = GetDatabaseBuilderWithProjectAndStudent
			(
				true /*buildable*/,
				GetFailedBuild(), 
				GetFailedBuild()
			).Build();

			var userId = database.Context.Users.First().Id;
			database.Reload();

			var projectService = GetProjectService(database.Context);
			var results = await projectService.GetProjectStatusAsync
			(
				"Class1",
				userId
			);

			Assert.Equal("LastName", results.LastName);
			Assert.Equal("FirstName", results.FirstName);
			Assert.Equal(userId, results.UserId);
			Assert.Equal(1, results.ProjectStatus.Count);
			Assert.Equal("Project1", results.ProjectStatus[0].ProjectName);
			Assert.False(results.ProjectStatus[0].LastBuildSucceeded);
			Assert.Equal(SecondPushDate, results.ProjectStatus[0].LastCommitDate);
			Assert.Equal(0, results.ProjectStatus[0].BuildTestCounts.Count);
		}

		/// <summary>
		/// Returns a failed build.
		/// </summary>
		private static Build GetFailedBuild()
		{
			return new Build() { Status = BuildStatus.Error };
		}

		/// <summary>
		/// Returns a successful build.
		/// </summary>
		private static Build GetSuccessfulBuild()
		{
			var successfulBuild = new Build()
			{
				Status = BuildStatus.Completed,
				TestResults = Collections.CreateList
				(
					new TestResult()
					{
						ClassName = "TestClass1",
						TestName = "TestClass1Test",
						Succeeded = true
					},
					new TestResult()
					{
						ClassName = "TestClass2",
						TestName = "TestClass2Test",
						Succeeded = false
					}
				)
			};
			return successfulBuild;
		}

		/// <summary>
		/// Returns a database with a project.
		/// </summary>
		private TestDatabaseBuilder GetDatabaseBuilderWithProject(
			bool explicitSubmissions = true)
		{
			return new TestDatabaseBuilder()
				.AddClassroom("Class1")
				.AddProject("Class1", "Project1", explicitSubmissions)
				.AddProjectTestClass("Class1", "Project1", "TestClass1")
				.AddProjectTestClass("Class1", "Project1", "TestClass2")
				.AddProjectPath("Class1", "Project1", "Immutable1", FileType.Immutable)
				.AddProjectPath("Class1", "Project1", "Immutable2", FileType.Immutable)
				.AddProjectPath("Class1", "Project1", "Private1", FileType.Private)
				.AddProjectPath("Class1", "Project1", "Private2", FileType.Private);
		}

		/// <summary>
		/// Returns a database with a project and a student.
		/// </summary>
		private TestDatabaseBuilder GetDatabaseBuilderWithProjectAndStudent(
			bool explicitSubmissions = true, 
			Build firstBuild = null, 
			Build secondBuild = null)
		{
			return GetDatabaseBuilderWithProject(explicitSubmissions)
				.AddSection("Class1", "Section1")
				.AddStudent("Student1", "LastName", "FirstName", "Class1", "Section1", "GitHubUser", inGitHubOrg: true)
				.AddCommit("Class1", "Project1", "Student1", "FirstCommitSha", FirstPushDate, firstBuild)
				.AddCommit("Class1", "Project1", "Student1", "SecondCommitSha", SecondPushDate, secondBuild);
		}

		/// <summary>
		/// Returns a project service instance.
		/// </summary>
		private IProjectService GetProjectService(
			DatabaseContext dbContext,
			IRepositoryPopulator repoPopulator = null, 
			IPushEventRetriever pushEventRetriever = null,
			IPushEventProcessor pushEventProcessor = null,
			IGitHubWebhookValidator webhookValidator = null,
			IJsonSerializer jsonSerializer = null,
			ITimeProvider timeProvider = null)
		{
			return new ProjectService
			(
				dbContext,
				repoPopulator,
				pushEventRetriever,
				pushEventProcessor,
				webhookValidator,
				jsonSerializer,
				timeProvider
			);
		}

		/// <summary>
		/// Returns a mock repository populator.
		/// </summary>
		private Mock<IRepositoryPopulator> GetMockRepositoryPopulator(
			IList<ProjectRepositoryFile> files = null,
			IList<CreateStudentRepoResult> createResults = null,
			bool overwriteIfSafe = false)
		{
			var repoPopulator = new Mock<IRepositoryPopulator>();

			repoPopulator
				.Setup
				(
					rp => rp.GetRepoFileListAsync
					(
						It.Is<Project>(project => IsExpectedProject(project))
					)
				).ReturnsAsync(files);

			repoPopulator
				.Setup
				(
					rp => rp.CreateReposAsync
					(
						It.Is<Project>(project => IsExpectedProject(project)),
						It.Is<IList<ClassroomMembership>>(students => AreExpectedStudents(students)),
						"WebhookUrl",
						overwriteIfSafe
					)
				).ReturnsAsync(createResults);

			return repoPopulator;
		}

		/// <summary>
		/// Returns whether the given project is the expected project
		/// we are using to test.
		/// </summary>
		private bool IsExpectedProject(Project project)
		{
			return project.Name == "Project1"
				&& project.Classroom.GitHubOrganization == "Class1GitHubOrg"
				&& project.ImmutableFilePaths.Count == 2
				&& project.ImmutableFilePaths[0].Path == "Immutable1"
				&& project.ImmutableFilePaths[1].Path == "Immutable2"
				&& project.PrivateFilePaths.Count == 2
				&& project.PrivateFilePaths[0].Path == "Private1"
				&& project.PrivateFilePaths[1].Path == "Private2";
		}

		/// <summary>
		/// Returns whether or not the given list of students are the
		/// expected students we are using to test.
		/// </summary>
		private bool AreExpectedStudents(IList<ClassroomMembership> students)
		{
			return students.Count == 1
				&& students[0].GitHubTeam == "LastNameFirstName";
		}

		/// <summary>
		/// Returns a mock push event processor.
		/// </summary>
		private Mock<IPushEventProcessor> GetMockPushEventProcessor(GitHubPushEvent pushEvent)
		{
			var pushEventProcessor = new Mock<IPushEventProcessor>();
			pushEventProcessor
				.Setup
				(
					processor => processor.GetNewCommitsToProcess
					(
						It.Is<Project>
						(
							project => project.Name == "Project1"
						),
						It.Is<ICollection<CommitDescriptor>>
						(
							existingCommits =>
								   existingCommits.Count == 2
								&& existingCommits.Any(c => c.Sha == "FirstCommitSha")
								&& existingCommits.Any(c => c.Sha == "SecondCommitSha")
						),
						It.Is<IList<StudentRepoPushEvents>>
						(
							events => 
								   events.Count == 1 
								&& events[0].PushEvents.Count == 1 
								&& events[0].PushEvents[0] == pushEvent	
						)
					)
				)
				.Returns<Project, ICollection<CommitDescriptor>, IList<StudentRepoPushEvents>>
				(
					(project, commits, pushEvents) => Collections.CreateList
					(
						new PushEventCommit
						(
							pushEvent,
							new Commit()
							{
								Sha = "NewCommitSha",
								PushDate = pushEvent.CreatedAt.UtcDateTime
							}
						)						
					)
				);

			pushEventProcessor
				.Setup(GetCreateBuildJobExpression(pushEvent))
				.ReturnsAsync("NewBuildJobId");

			return pushEventProcessor;
		}

		/// <summary>
		/// Returns a mock push event retriever.
		/// </summary>
		private Mock<IPushEventRetriever> GetMockPushEventRetriever(GitHubPushEvent pushEvent)
		{
			var pushEventRetriever = new Mock<IPushEventRetriever>();
			pushEventRetriever
				.Setup
				(
					retriever => retriever.GetAllPushEventsAsync
					(
						It.Is<Project>
						(
							project => project.Name == "Project1"
						),
						It.Is<IList<ClassroomMembership>>
						(
							students =>
								students.Count == 1
								&& students[0].GitHubTeam == "LastNameFirstName"
						)
					)
				)
				.Returns<Project, IList<ClassroomMembership>>
				(
					(project, students) => Task.FromResult<IList<StudentRepoPushEvents>>
					(
						Collections.CreateList
						(
							new StudentRepoPushEvents(students[0], Collections.CreateList(pushEvent))
						)
					)
				);

			return pushEventRetriever;
		}

		/// <summary>
		/// Returns a mock json serializer.
		/// </summary>
		private static Mock<IJsonSerializer> GetMockJsonSerializer(
			GitHubPushEvent pushEvent)
		{
			var jsonSerializer = new Mock<IJsonSerializer>();

			jsonSerializer
				.Setup(js => js.Deserialize<GitHubPushEvent>("SerializedPushEvent"))
				.Returns(pushEvent);

			return jsonSerializer;
		}

		/// <summary>
		/// Returns a mock time provider.
		/// </summary>
		private static Mock<ITimeProvider> GetMockTimeProvider(
			DateTime now)
		{
			var timeProvider = new Mock<ITimeProvider>();

			timeProvider
				.Setup(tp => tp.UtcNow)
				.Returns(DateTime.SpecifyKind(now, DateTimeKind.Utc));

			return timeProvider;
		}

		/// <summary>
		/// Returns the expected expression for CreateRepositoryAsync.
		/// </summary>
		private static Expression<Func<IPushEventProcessor, Task<string>>> GetCreateBuildJobExpression(
			GitHubPushEvent pushEvent)
		{
			return processor => processor.CreateBuildJobAsync
			(
				It.Is<Project>
				(
					project => project.Name == "Project1"	
				),
				It.Is<PushEventCommit>
				(
					pushEventCommit => pushEventCommit.PushEvent == pushEvent	
				),
				"BuildResultCallbackUrl"
			);
		}

		/// <summary>
		/// Returns a new push event.
		/// </summary>
		private static GitHubPushEvent GetPushEvent(string refPath)
		{
			return new GitHubPushEvent()
			{
				Ref = refPath,
				Repository = new PushEventRepository()
				{
					Name = "Project1_LastNameFirstName"
				}
			};
		}

		/// <summary>
		/// The push date of the first commit.
		/// </summary>
		private static readonly DateTime FirstPushDate = new DateTime(2016, 1, 1);

		/// <summary>
		/// The push date of the second commit.
		/// </summary>
		private static readonly DateTime SecondPushDate = new DateTime(2016, 1, 2);
	}
}
