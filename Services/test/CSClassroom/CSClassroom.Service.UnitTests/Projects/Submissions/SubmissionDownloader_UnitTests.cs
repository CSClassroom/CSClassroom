using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.GitHub;
using CSC.Common.Infrastructure.System;
using CSC.Common.TestDoubles;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Projects.Repositories;
using CSC.CSClassroom.Service.Projects.Submissions;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Projects.Submissions
{
	/// <summary>
	/// Unit tests for the submission downloader class.
	/// </summary>
	public class SubmissionDownloader_UnitTests
	{
		/// <summary>
		/// Ensures that the submission 
		/// </summary>
		[Fact]
		public async Task DownloadProjectTemplateAsync_DownloadsTemplate()
		{
			var classroom = GetClassroom();
			var project = GetProject(classroom);

			var templateRepo = new RepositoryContents
			(
				"Project1_Template", 
				null /*branchName*/, 
				ArchiveStore.Memory
			);

			var repoClient = GetMockGitHubRepositoryClient(templateRepo);
			var downloader = GetSubmissionDownloader(repoClient);

			var result = await downloader.DownloadTemplateContentsAsync(project);

			Assert.Equal(result, templateRepo.Contents);
		}

		/// <summary>
		/// Ensures that a student's valid submission is correctly downloaded.
		/// </summary>
		[Fact]
		public async Task DownloadSubmissionsAsync_ValidSubmission_DownloadsSubmission()
		{
			var classroom = GetClassroom();
			var project = GetProject(classroom);
			var checkpoint = GetCheckpoint(project);
			var student = GetStudent(classroom, "Student1");

			var studentRepo = new RepositoryContents
			(
				"Project1_Student1",
				"Checkpoint1",
				ArchiveStore.FileSystem
			);

			var repoClient = GetMockGitHubRepositoryClient(studentRepo);
			var repoMetadataRetriever = GetMockRepositoryMetadataRetriever();
			var downloader = GetSubmissionDownloader
			(
				repoClient, 
				repoMetadataRetriever
			);

			var result = await downloader.DownloadSubmissionsAsync
			(
				checkpoint,
				Collections.CreateList
				(
					new StudentDownloadRequest(student, submitted: true)
				)
			);

			Assert.Single(result);
			Assert.Equal(student, result[0].Student);
			Assert.Equal(studentRepo.Contents, result[0].Contents);
		}

		/// <summary>
		/// Ensures that we do not download a student's submission if there is
		/// no repo for that student.
		/// </summary>
		[Fact]
		public async Task DownloadSubmissionsAsync_NoRepo_SubmissionNotDownloaded()
		{
			var classroom = GetClassroom();
			var project = GetProject(classroom);
			var checkpoint = GetCheckpoint(project);
			var student1 = GetStudent(classroom, "Student1");
			var student2 = GetStudent(classroom, "Student2");

			var studentRepo1 = new RepositoryContents
			(
				"Project1_Student1",
				"Checkpoint1",
				ArchiveStore.FileSystem,
				contents: true
			);

			var studentRepo2 = new RepositoryContents
			(
				"Project1_Student2",
				"Checkpoint1",
				ArchiveStore.FileSystem,
				contents: false
			);

			var repoClient = GetMockGitHubRepositoryClient(studentRepo1, studentRepo2);
			var repoMetadataRetriever = GetMockRepositoryMetadataRetriever();
			var downloader = GetSubmissionDownloader
			(
				repoClient,
				repoMetadataRetriever
			);

			var result = await downloader.DownloadSubmissionsAsync
			(
				checkpoint,
				Collections.CreateList
				(
					new StudentDownloadRequest(student1, submitted: true),
					new StudentDownloadRequest(student2, submitted: false)
				)
			);

			Assert.Single(result);
			Assert.Equal(student1, result[0].Student);
			Assert.Equal(studentRepo1.Contents, result[0].Contents);
		}

		/// <summary>
		/// Ensures that a student's latest commit is correctly downloaded when
		/// the student did not make a submission for a checkpoint.
		/// </summary>
		[Fact]
		public async Task DownloadSubmissionsAsync_NoSubmission_DownloadsLatestCommit()
		{
			var classroom = GetClassroom();
			var project = GetProject(classroom);
			var checkpoint = GetCheckpoint(project);
			var student = GetStudent(classroom, "Student1");

			var studentRepo = new RepositoryContents
			(
				"Project1_Student1",
				null /*branchName*/,
				ArchiveStore.FileSystem
			);

			var repoClient = GetMockGitHubRepositoryClient(studentRepo);
			var repoMetadataRetriever = GetMockRepositoryMetadataRetriever();
			var downloader = GetSubmissionDownloader
			(
				repoClient,
				repoMetadataRetriever
			);

			var result = await downloader.DownloadSubmissionsAsync
			(
				checkpoint,
				Collections.CreateList
				(
					new StudentDownloadRequest(student, submitted: false)
				)
			);

			Assert.Single(result);
			Assert.Equal(student, result[0].Student);
			Assert.Equal(studentRepo.Contents, result[0].Contents);
		}

		/// <summary>
		/// Returns a classroom.
		/// </summary>
		private Classroom GetClassroom()
		{
			return new Classroom()
			{
				Name = "Class1",
				GitHubOrganization = "GitHubOrg"
			};
		}

		/// <summary>
		/// Returns a project.
		/// </summary>
		private Project GetProject(Classroom classroom)
		{
			return new Project()
			{
				Name = "Project1",
				Classroom = classroom,
				ClassroomId = classroom.Id
			};
		}

		/// <summary>
		/// Returns a user.
		/// </summary>
		private ClassroomMembership GetStudent(
			Classroom classroom, 
			string team)
		{
			return new ClassroomMembership()
			{
				Classroom = classroom,
				ClassroomId = classroom.Id,
				GitHubTeam = team
			};
		}

		/// <summary>
		/// Returns a list of classroom memberships for a set of users.
		/// </summary>
		private IList<StudentDownloadRequest> GetDownloadRequests(
			params ClassroomMembership[] students)
		{
			return students
				.Select
				(
					student => new StudentDownloadRequest
					(
						student,
						submitted: true
					)
				).ToList();
		}

		/// <summary>
		/// Returns a checkpoint.
		/// </summary>
		private Checkpoint GetCheckpoint(Project project)
		{
			return new Checkpoint()
			{
				Name = "Checkpoint1",
				DisplayName = "Checkpoint 1",
				Project = project,
				ProjectId = project.Id
			};
		}

		/// <summary>
		/// Creates a repository metadata retriever.
		/// </summary>
		private IRepositoryMetadataRetriever GetMockRepositoryMetadataRetriever()
		{
			var repoMetadataRetriever = new Mock<IRepositoryMetadataRetriever>();

			repoMetadataRetriever
				.Setup
				(
					rmr => rmr.GetRepoName
					(
						It.IsAny<Project>(),
						It.IsAny<ClassroomMembership>()
					)
				).Returns<Project, ClassroomMembership>
				(
					(project, student) => $"{project.Name}_{student.GitHubTeam}"
				);

			return repoMetadataRetriever.Object;
		}

		/// <summary>
		/// Creates a GitHub repository client that will return the contents of
		/// the given repositories.
		/// </summary>
		private IGitHubRepositoryClient GetMockGitHubRepositoryClient(
			params RepositoryContents[] repositories)
		{
			var repoClient = new Mock<IGitHubRepositoryClient>();

			foreach (var repository in repositories)
			{
				if (repository.Contents != null)
				{
					repoClient
						.Setup
						(
							rc => rc.GetRepositoryContentsAsync
							(
								"GitHubOrg",
								repository.RepoName,
								repository.BranchName,
								repository.BackingStore
							)
						).ReturnsAsync(repository.Contents);
				}
				else
                {
					repoClient
						.Setup
						(
							rc => rc.GetRepositoryContentsAsync
							(
								"GitHubOrg",
								repository.RepoName,
								repository.BranchName,
								repository.BackingStore
							)
						)
						.ThrowsAsync
						(
							new Octokit.NotFoundException
							(
								"Not found",
								System.Net.HttpStatusCode.NotFound
							)
						);
				}
			}

			return repoClient.Object;
		}

		/// <summary>
		/// Returns a submission downloader.
		/// </summary>
		private ISubmissionDownloader GetSubmissionDownloader(
			IGitHubRepositoryClient repoClient = null,
			IRepositoryMetadataRetriever repoMetadataRetriever = null)
		{
			return new SubmissionDownloader
			(
				repoMetadataRetriever,
				repoClient,
				new MockOperationRunner()
			);
		}

		/// <summary>
		/// Represents the contents of a repository.
		/// </summary>
		private class RepositoryContents
		{
			/// <summary>
			/// The repository name.
			/// </summary>
			public string RepoName { get; }

			/// <summary>
			/// The branch name.
			/// </summary>
			public string BranchName { get; }

			/// <summary>
			/// The backing store for the zip file.
			/// </summary>
			public ArchiveStore BackingStore { get; }
			 
			/// <summary>
			/// The files in the repository.
			/// </summary>
			public IArchive Contents { get; }

			/// <summary>
			/// Constructor.
			/// </summary>
			public RepositoryContents(
				string repoName,
				string branchName,
				ArchiveStore backingStore,
				bool contents = true)
			{
				RepoName = repoName;
				BranchName = branchName;
				BackingStore = backingStore;
				if (contents)
				{
					Contents = new UncompressedMemoryArchive
					(
						new Dictionary<string, byte[]>()
					);
				}
			}
		}
	}
}
