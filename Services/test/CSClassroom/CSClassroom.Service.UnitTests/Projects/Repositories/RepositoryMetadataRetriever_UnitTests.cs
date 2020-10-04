using System.Threading.Tasks;
using CSC.Common.Infrastructure.GitHub;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Projects.Repositories;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Projects.Repositories
{
	/// <summary>
	/// Unit tests for the RepositoryMetadataRetriever class.
	/// </summary>
	public class RepositoryMetadataRetriever_UnitTests
	{
		/// <summary>
		/// Ensures that GetRepoName returns the correct name.
		/// </summary>
		[Fact]
		public void GetRepoName_ReturnsCorrectName()
		{
			var repoMetadataRetriever = new RepositoryMetadataRetriever(repoClient: null);
			
			var project = new Project() { Name = "Project1" };
			var student = new ClassroomMembership() { GitHubTeam = "LastNameFirstName" };

			var result = repoMetadataRetriever.GetRepoName(project, student);

			Assert.Equal("Project1_LastNameFirstName", result);
		}

		/// <summary>
		/// Ensures that GetStudentRepositoriesAsync returns the correct repositories.
		/// </summary>
		[Fact]
		public async Task GetStudentRepositoriesAsync_ReturnsCorrectRepositories()
		{
			var project = new Project()
			{
				Name = "Project1",
				Classroom = new Classroom() { GitHubOrganization = "GitHubOrg" }
			};

			var students = Collections.CreateList
			(
				new ClassroomMembership() { GitHubTeam = "Last1First1" },
				new ClassroomMembership() { GitHubTeam = "Last2First2" }
			);

			var reposInOrganization = Collections.CreateList
			(
				new GitHubRepository(0, "GitHubOrg", "Project1_Last1First1", "main"),
				new GitHubRepository(1, "GitHubOrg", "Project1_Last2First2", "main"),
				new GitHubRepository(2, "GitHubOrg", "SomeOtherProject_Last1First1", "main"),
				new GitHubRepository(3, "GitHubOrg", "SomeOtherProject_Last3First3", "main")
			);

			var repoClient = new Mock<IGitHubRepositoryClient>();
			repoClient
				.Setup(rc => rc.GetAllRepositoriesAsync("GitHubOrg"))
				.ReturnsAsync(reposInOrganization);

			var repoMetadataRetriever = new RepositoryMetadataRetriever(repoClient.Object);

			var results = await repoMetadataRetriever.GetStudentRepositoriesAsync
			(
				project,
				students
			);

			Assert.Equal(2, results.Count);
			Assert.Equal(0, results[students[0]].Id);
			Assert.Equal(1, results[students[1]].Id);
		}
	}
}
