using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.GitHub;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Projects.PushEvents;
using CSC.CSClassroom.Service.Projects.Repositories;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Projects.PushEvents
{
	/// <summary>
	/// Unit tests for the PushEventRetriever class.
	/// </summary>
	public class PushEventRetriever_UnitTests
	{
		/// <summary>
		/// Ensures that GetAllPushEventsAsync actually returns all push events.
		/// </summary>
		[Fact]
		public async Task GetAllPushEventsAsync_ReturnsPushEvents()
		{
			var project = new Project();
			var students = Collections.CreateList(new ClassroomMembership());
			var pushEvents = Collections.CreateList<GitHubPushEvent>();

			var repoMetadataRetriever = new Mock<IRepositoryMetadataRetriever>();
			repoMetadataRetriever
				.Setup(rmr => rmr.GetStudentRepositoriesAsync(project, students))
				.ReturnsAsync
				(
					new Dictionary<ClassroomMembership, GitHubRepository>()
					{
						[students[0]] = new GitHubRepository(1, "GitHubOrg", "GitHubRepoName")
					}
				);

			var repoClient = new Mock<IGitHubRepositoryClient>();
			repoClient
				.Setup(rc => rc.GetPushEventsAsync("GitHubOrg", "GitHubRepoName"))
				.ReturnsAsync(pushEvents);

			var operationRunner = new MockOperationRunner();

			var pushEventRetriever = new PushEventRetriever
			(
				repoMetadataRetriever.Object,
				repoClient.Object,
				operationRunner
			);

			var results = await pushEventRetriever.GetAllPushEventsAsync(project, students);

			Assert.Equal(results.Count, 1);
			Assert.Equal(students[0], results[0].Student);
			Assert.Equal(pushEvents, results[0].PushEvents);
		}
	}
}
