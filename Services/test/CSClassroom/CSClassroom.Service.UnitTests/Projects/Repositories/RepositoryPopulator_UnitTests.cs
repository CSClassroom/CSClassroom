using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.GitHub;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Projects.Repositories;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Projects.Repositories
{
	/// <summary>
	/// Unit tests for the RepositoryPopulator class.
	/// </summary>
	public class RepositoryPopulator_UnitTests
	{
		/// <summary>
		/// Ensures GetRepositoryFileListAsync returns the file list for a project.
		/// </summary>
		[Fact]
		public async Task GetRepositoryFileListAsync_NewRepo_ReturnsResult()
		{
			var repoClient = GetMockGitHubRepositoryClient
			(
				GetProjectTemplate(),
				existingStudentRepo: false,
				numCommits: 0
			);

			var project = GetProject();

			var repoPopulator = GetRepositoryPopulator
			(
				repoClient: repoClient.Object
			);

			var result = await repoPopulator.GetRepoFileListAsync(project);

			result = result.OrderBy(file => file.Path).ToList();

			Assert.Equal(5, result.Count);
			Assert.Equal("Immutable1/ImmutableFile1.txt", result[0].Path);
			Assert.Equal(FileType.Immutable, result[0].FileType);
			Assert.Equal("Immutable2/ImmutableFile2.txt", result[1].Path);
			Assert.Equal(FileType.Immutable, result[1].FileType);
			Assert.Equal("Private1/PrivateFile1.txt", result[2].Path);
			Assert.Equal(FileType.Private, result[2].FileType);
			Assert.Equal("Private2/PrivateFile2.txt", result[3].Path);
			Assert.Equal(FileType.Private, result[3].FileType);
			Assert.Equal("Public/PublicFile1.txt", result[4].Path);
			Assert.Equal(FileType.Public, result[4].FileType);
		}

		/// <summary>
		/// Ensures CreateStudentRepositoriesAsync creates a new repository when
		/// no there is existing repository.
		/// </summary>
		[Fact]
		public async Task CreateStudentRepositoriesAsync_NoExistingRepo_CreatesRepo()
		{
			var project = GetProject();
			var students = GetStudents();
			var template = GetProjectTemplate();
			var teamClient = GetMockGitHubTeamClient();
			var repoClient = GetMockGitHubRepositoryClient
			(
				template,
				existingStudentRepo: false,
				numCommits: 0
			);

			var repoPopulator = GetRepositoryPopulator
			(
				teamClient: teamClient.Object,
				repoClient: repoClient.Object
			);
			
			var result = await repoPopulator.CreateReposAsync
			(
				project,
				students,
				"WebhookUrl",
				overwriteIfSafe: false
			);

			Assert.Equal(1, result.Count);
			Assert.Equal(students[0].User, result[0].Student);
			Assert.Equal(CreateAndPushResult.Created, result[0].CreateAndPushResult);

			repoClient.Verify(GetCreateRepositoryExpression(), Times.Once);
			repoClient.Verify(GetOverwriteRepositoryExpression(template), Times.Once);
			repoClient.Verify(GetEnsurePushWebhookExpression(), Times.Once);
		}

		/// <summary>
		/// Ensures CreateStudentRepositoriesAsync does not create a new repository when
		/// there is an existing repository, if repo overwrite is not desired.
		/// </summary>
		[Fact]
		public async Task CreateStudentRepositoriesAsync_ExistingRepoNoOverwrite_DoesntCreateRepo()
		{
			var project = GetProject();
			var students = GetStudents();
			var template = GetProjectTemplate();
			var teamClient = GetMockGitHubTeamClient();
			var repoClient = GetMockGitHubRepositoryClient
			(
				template,
				existingStudentRepo: true,
				numCommits: 0
			);

			var repoPopulator = GetRepositoryPopulator
			(
				teamClient: teamClient.Object,
				repoClient: repoClient.Object
			);

			var result = await repoPopulator.CreateReposAsync
			(
				project,
				students,
				"WebhookUrl",
				overwriteIfSafe: false
			);

			Assert.Equal(1, result.Count);
			Assert.Equal(students[0].User, result[0].Student);
			Assert.Equal(CreateAndPushResult.Exists, result[0].CreateAndPushResult);

			repoClient.Verify(GetCreateRepositoryExpression(), Times.Never);
			repoClient.Verify(GetOverwriteRepositoryExpression(template), Times.Never);
			repoClient.Verify(GetEnsurePushWebhookExpression(), Times.Never);
		}

		/// <summary>
		/// Ensures CreateStudentRepositoriesAsync does not create a new repository when
		/// there is an existing repository, if it would not be safe to overwrite the repo
		/// (even if overwrite is desired). It is not safe to overwrite the repo when the
		/// repo has any commits beyond the initial commits.
		/// </summary>
		[Fact]
		public async Task CreateStudentRepositoriesAsync_ExistingRepoUnsafeToOverwrite_DoesntCreateRepo()
		{
			var project = GetProject();
			var students = GetStudents();
			var template = GetProjectTemplate();
			var teamClient = GetMockGitHubTeamClient();
			var repoClient = GetMockGitHubRepositoryClient
			(
				template,
				existingStudentRepo: true,
				numCommits: 3
			);

			var repoPopulator = GetRepositoryPopulator
			(
				teamClient: teamClient.Object,
				repoClient: repoClient.Object
			);

			var result = await repoPopulator.CreateReposAsync
			(
				project,
				students,
				"WebhookUrl",
				overwriteIfSafe: true
			);

			Assert.Equal(1, result.Count);
			Assert.Equal(students[0].User, result[0].Student);
			Assert.Equal(CreateAndPushResult.Exists, result[0].CreateAndPushResult);

			repoClient.Verify(GetCreateRepositoryExpression(), Times.Never);
			repoClient.Verify(GetOverwriteRepositoryExpression(template), Times.Never);
			repoClient.Verify(GetEnsurePushWebhookExpression(), Times.Never);
		}

		/// <summary>
		/// Ensures CreateStudentRepositoriesAsync does create a new repository when
		/// there is an existing repository, if overwrite is desired and it is safe to
		/// overwrite the repo.
		/// </summary>
		[Fact]
		public async Task CreateStudentRepositoriesAsync_ExistingRepoSafeToOverwrite_OverwritesRepo()
		{
			var project = GetProject();
			var students = GetStudents();
			var template = GetProjectTemplate();
			var teamClient = GetMockGitHubTeamClient();
			var repoClient = GetMockGitHubRepositoryClient
			(
				template,
				existingStudentRepo: true,
				numCommits: 2
			);

			var repoPopulator = GetRepositoryPopulator
			(
				teamClient: teamClient.Object,
				repoClient: repoClient.Object
			);

			var result = await repoPopulator.CreateReposAsync
			(
				project,
				students,
				"WebhookUrl",
				overwriteIfSafe: true
			);

			Assert.Equal(1, result.Count);
			Assert.Equal(students[0].User, result[0].Student);
			Assert.Equal(CreateAndPushResult.Overwritten, result[0].CreateAndPushResult);

			repoClient.Verify(GetCreateRepositoryExpression(), Times.Never);
			repoClient.Verify(GetOverwriteRepositoryExpression(template), Times.Once);
			repoClient.Verify(GetEnsurePushWebhookExpression(), Times.Once);
		}

		/// <summary>
		/// Ensures that EnsureWebhooksPresentAsync ensures that all student repositories
		/// have a webhook.
		/// </summary>
		[Fact]
		public async Task EnsureWebhooksPresentAsync_EnsuresAllWebhooksPresent()
		{
			var project = GetProject();
			var students = GetStudents();
			var repoClient = GetMockGitHubRepositoryClient();
			var repoMetadataRetriever = GetMockRepositoryMetadataRetriever(project, students);

			var repoPopulator = GetRepositoryPopulator
			(
				repoMetadataRetriever: repoMetadataRetriever.Object,
				repoClient: repoClient.Object
			);

			await repoPopulator.EnsureWebHooksPresentAsync(project, students, "WebhookUrl");

			repoClient.Verify(GetEnsurePushWebhookExpression(), Times.Once);
		}

		/// <summary>
		/// Creates a repository populator.
		/// </summary>
		private IRepositoryPopulator GetRepositoryPopulator(
			IRepositoryMetadataRetriever repoMetadataRetriever = null,
			IGitHubTeamClient teamClient = null,
			IGitHubRepositoryClient repoClient = null)
		{
			var operationRunner = new MockOperationRunner();

			return new RepositoryPopulator
			(
				new Mock<ILogger<RepositoryPopulator>>().Object,
				repoMetadataRetriever,
				teamClient,
				repoClient,
				operationRunner
			);
		}

		/// <summary>
		/// Creates a repository metadata retriever.
		/// </summary>
		private Mock<IRepositoryMetadataRetriever> GetMockRepositoryMetadataRetriever(
			Project project, 
			IList<ClassroomMembership> students)
		{
			var repoMetadataRetriever = new Mock<IRepositoryMetadataRetriever>();
			repoMetadataRetriever
				.Setup
				(
					rmr => rmr.GetStudentRepositoriesAsync(project, students)
				)
				.ReturnsAsync
				(
					students.ToDictionary
					(
						student => student,
						student => new GitHubRepository
						(
							id: 0, 
							owner: "Class1GitHubOrg", 
							name: $"{project.Name}_{student.GitHubTeam}"
						)
					)
				);

			return repoMetadataRetriever;
		}

		/// <summary>
		/// Returns a project.
		/// </summary>
		private Project GetProject()
		{
			return new Project()
			{
				Name = "Project1",
				Classroom = new Classroom()
				{
					GitHubOrganization = "Class1GitHubOrg"
				},
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
		/// Returns a set of students.
		/// </summary>
		private IList<ClassroomMembership> GetStudents()
		{
			return Collections.CreateList
			(
				new ClassroomMembership()
				{
					User = new User(),
					GitHubTeam = "LastNameFirstName"
				}
			);
		}

		/// <summary>
		/// Returns a mock team client.
		/// </summary>
		private Mock<IGitHubTeamClient> GetMockGitHubTeamClient()
		{
			var teamClient = new Mock<IGitHubTeamClient>();
			teamClient
				.Setup(tc => tc.GetAllTeamsAsync("Class1GitHubOrg"))
				.ReturnsAsync(Collections.CreateList(new GitHubTeam(1, "LastNameFirstName")));

			return teamClient;
		}

		/// <summary>
		/// Returns a mock GitHub repository client.
		/// </summary>
		private Mock<IGitHubRepositoryClient> GetMockGitHubRepositoryClient(
			IArchive templateContents = null,
			bool existingStudentRepo = false,
			int numCommits = 0)
		{
			var repoClient = new Mock<IGitHubRepositoryClient>();

			SetupGetRepositoryContentsAsync(repoClient, templateContents);
			SetupGetAllRepositoriesAsync(repoClient, existingStudentRepo);
			SetupGetAllCommitsAsync(repoClient, numCommits);
			SetupCreateRepositoryAsync(repoClient);
			SetupOverwriteRepositoryAsync(repoClient, templateContents);
			SetupEnsurePushWebhookAsync(repoClient);

			return repoClient;
		}

		/// <summary>
		/// Sets up GetRepositoryContentsAsync for the template repository.
		/// </summary>
		private void SetupGetRepositoryContentsAsync(
			Mock<IGitHubRepositoryClient> repoClient,
			IArchive templateContents)
		{
			repoClient
				.Setup
				(
					rc => rc.GetRepositoryContentsAsync
					(
						"Class1GitHubOrg",
						"Project1_Template",
						null /*branchName*/,
						ArchiveStore.Memory
					)
				).ReturnsAsync(templateContents);
		}

		/// <summary>
		/// Sets up GetAllRepositoriesAsync.
		/// </summary>
		private static void SetupGetAllRepositoriesAsync(
			Mock<IGitHubRepositoryClient> repoClient,
			bool existingStudentRepo)
		{
			var repo = new GitHubRepository(1, "GitHubUser", "Project1_LastNameFirstName");

			repoClient
				.Setup(rc => rc.GetAllRepositoriesAsync("Class1GitHubOrg"))
				.ReturnsAsync
				(
					existingStudentRepo
						? Collections.CreateList(repo)
						: new List<GitHubRepository>()
				);
		}

		/// <summary>
		/// Sets up GetAllCommitsAsync.
		/// </summary>
		private static void SetupGetAllCommitsAsync(
			Mock<IGitHubRepositoryClient> repoClient,
			int numCommits)
		{
			repoClient
				.Setup
				(
					rc => rc.GetAllCommitsAsync
					(
						"Class1GitHubOrg",
						"Project1_LastNameFirstName"
					)
				).ReturnsAsync
				(
					new int[numCommits].Select
					(
						(value, index) => new GitHubCommit
						(
							$"Sha{index + 1}",
							"Class1GitHubOrg",
							"Project1_LastNameFirstName",
							"GitHubUser",
							$"Commit {index + 1}",
							DateTimeOffset.UtcNow + TimeSpan.FromSeconds(index),
							index == 0
								? new List<string>()
								: Collections.CreateList($"Sha{index}")
						)
					).ToList()
				);
		}

		/// <summary>
		/// Sets up CreateRepositoryAsync.
		/// </summary>
		private static void SetupCreateRepositoryAsync(
			Mock<IGitHubRepositoryClient> repoClient)
		{
			var repo = new GitHubRepository(1, "GitHubUser", "Project1_LastNameFirstName");

			repoClient
				.Setup(GetCreateRepositoryExpression())
				.ReturnsAsync(repo);
		}

		/// <summary>
		/// Returns the expected expression for CreateRepositoryAsync.
		/// </summary>
		private static Expression<Func<IGitHubRepositoryClient, Task<GitHubRepository>>> GetCreateRepositoryExpression()
		{
			return rc => rc.CreateRepositoryAsync
			(
				"Class1GitHubOrg",
				"Project1_LastNameFirstName",
				It.Is<GitHubTeam>(t => t.Name == "LastNameFirstName"),
				false /*overwrite*/
			);
		}

		/// <summary>
		/// Sets up OverwriteRepositoryAsync.
		/// </summary>
		private static void SetupOverwriteRepositoryAsync(
			Mock<IGitHubRepositoryClient> repoClient,
			IArchive zipFile)
		{
			repoClient
				.Setup(GetOverwriteRepositoryExpression(zipFile))
				.Returns(Task.CompletedTask);
		}

		/// <summary>
		/// Returns the expression for overwriting a repository.
		/// </summary>
		private static Expression<Func<IGitHubRepositoryClient, Task>> GetOverwriteRepositoryExpression(
			IArchive contents)
		{
			return gc => gc.OverwriteRepositoryAsync
			(
				It.Is<GitHubRepository>(r => r.Name == "Project1_LastNameFirstName"),
				It.IsAny<string>(),
				contents,
				It.IsAny<Func<IArchiveFile, bool>>(),
				It.IsAny<Func<IArchiveFile, bool>>()
			);
		}


		/// <summary>
		/// Sets up EnsurePushWebhookAsync.
		/// </summary>
		private static void SetupEnsurePushWebhookAsync(
			Mock<IGitHubRepositoryClient> repoClient)
		{
			repoClient
				.Setup(GetEnsurePushWebhookExpression())
				.Returns(Task.CompletedTask);
		}


		/// <summary>
		/// Returns the expression for overwriting a repository.
		/// </summary>
		private static Expression<Func<IGitHubRepositoryClient, Task>> GetEnsurePushWebhookExpression()
		{
			return gc => gc.EnsurePushWebhookAsync
			(
				It.Is<GitHubRepository>(r => r.Name == "Project1_LastNameFirstName"),
				"WebhookUrl"
			);
		}

		/// <summary>
		/// Returns a project template.
		/// </summary>
		private IArchive GetProjectTemplate()
		{
			return new UncompressedMemoryArchive
			(
				new Dictionary<string, byte[]>()
				{
					["Public/PublicFile1.txt"] = new byte[0],
					["Immutable1/ImmutableFile1.txt"] = new byte[0],
					["Immutable2/ImmutableFile2.txt"] = new byte[0],
					["Private1/PrivateFile1.txt"] = new byte[0],
					["Private2/PrivateFile2.txt"] = new byte[0],
				}	
			);
		}
	}
}
