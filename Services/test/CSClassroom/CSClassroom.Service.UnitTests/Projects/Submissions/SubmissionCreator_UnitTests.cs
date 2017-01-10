using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.GitHub;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.Service.Projects.Submissions;
using CSC.CSClassroom.Service.UnitTests.TestDoubles;
using CSC.CSClassroom.Service.UnitTests.Utilities;
using Moq;
using Xunit;

namespace CSC.CSClassroom.Service.UnitTests.Projects.Submissions
{
	/// <summary>
	/// Unit tests for the SubmissionCreator class.
	/// </summary>
	public class SubmissionCreator_UnitTests
	{
		/// <summary>
		/// Ensures that GetSubmissionCandidatesAsync returns all candidate
		/// commits for submission.
		/// </summary>
		[Fact]
		public async Task GetSubmissionCandidatesAsync_ReturnsCandidates()
		{
			var classroom = GetClassroom();
			var project = GetProject(classroom);
			var user = GetUser(classroom);

			var repoClient = GetMockRepositoryClient();
			var submissionCreator = new SubmissionCreator(repoClient.Object);

			var results = await submissionCreator.GetSubmissionCandidatesAsync
			(
				project, 
				user
			);

			Assert.Equal(2, results.Count);
			Assert.True(results.Contains("Commit2"));
			Assert.True(results.Contains("Commit3"));
		}

		/// <summary>
		/// Ensures that CreatePullRequestAsync creates the destination branch for 
		/// the pull request. 
		/// </summary>
		[Fact]
		public async Task CreatePullRequestAsync_CreatesDestinationBranch()
		{
			var classroom = GetClassroom();
			var project = GetProject(classroom);
			var user = GetUser(classroom);
			var checkpoint = GetCheckpoint(project);
			var commit = GetCommit(user, project);

			var repoClient = GetMockRepositoryClient();
			var submissionCreator = new SubmissionCreator(repoClient.Object);

			await submissionCreator.CreatePullRequestAsync(commit, checkpoint);

			repoClient.Verify(GetCreateBranchExpression("Checkpoint1", "Commit1"), Times.Once);
		}

		/// <summary>
		/// Ensures that CreatePullRequestAsync creates and deletes the temporary
		/// source branch for the pull request. 
		/// </summary>
		[Fact]
		public async Task CreatePullRequestAsync_CreatesAndDeletesSourceBranch()
		{
			var classroom = GetClassroom();
			var project = GetProject(classroom);
			var user = GetUser(classroom);
			var checkpoint = GetCheckpoint(project);
			var commit = GetCommit(user, project);

			var repoClient = GetMockRepositoryClient();
			var submissionCreator = new SubmissionCreator(repoClient.Object);

			await submissionCreator.CreatePullRequestAsync(commit, checkpoint);
			
			repoClient.Verify(GetCreateBranchExpression("Checkpoint1Source", "Commit3"), Times.Once);
			repoClient.Verify(GetDeleteBranchExpression("Checkpoint1Source"), Times.Once);
		}

		/// <summary>
		/// Ensures that CreatePullRequestAsync actually creates a pull request, and
		/// returns its id.
		/// </summary>
		[Fact]
		public async Task CreatePullRequestAsync_CreatesPullRequest()
		{
			var classroom = GetClassroom();
			var project = GetProject(classroom);
			var user = GetUser(classroom);
			var checkpoint = GetCheckpoint(project);
			var commit = GetCommit(user, project);

			var repoClient = GetMockRepositoryClient(pullRequestId: 123);
			var submissionCreator = new SubmissionCreator(repoClient.Object);

			var result = await submissionCreator.CreatePullRequestAsync(commit, checkpoint);

			repoClient.Verify(GetCreatePullRequestExpression("Checkpoint1Source", "Checkpoint1"), Times.Once);

			Assert.Equal(123, result);
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
		private User GetUser(Classroom classroom)
		{
			return new User()
			{
				ClassroomMemberships = Collections.CreateList
				(
					new ClassroomMembership()
					{
						Classroom = classroom,
						ClassroomId = classroom.Id,
						GitHubTeam = "LastNameFirstName"
					}
				)
			};
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
		/// Returns a commit.
		/// </summary>
		private Commit GetCommit(User user, Project project)
		{
			return new Commit()
			{
				Sha = "Commit3",
				User = user,
				UserId = user.Id,
				Project = project,
				ProjectId = project.Id
			};
		}

		/// <summary>
		/// Returns a mock repository client.
		/// </summary>
		private Mock<IGitHubRepositoryClient> GetMockRepositoryClient(int pullRequestId = 0)
		{
			var repoClient = new Mock<IGitHubRepositoryClient>();

			repoClient
				.Setup(rc => rc.GetAllCommitsAsync("GitHubOrg", "Project1_LastNameFirstName"))
				.ReturnsAsync
				(
					Collections.CreateList
					(
						new GitHubCommit("Commit1", "GitHubOrg", "Project1_LastNameFirstName",
							"User", "Message1", DateTime.Now, parents: new List<string>()),

						new GitHubCommit("Commit2", "GitHubOrg", "Project1_LastNameFirstName",
							"User", "Message2", DateTime.Now, parents: Collections.CreateList("Commit1")),

						new GitHubCommit("Commit3", "GitHubOrg", "Project1_LastNameFirstName",
							"User", "Message3", DateTime.Now, parents: Collections.CreateList("Commit2"))
					)
				);

			repoClient
				.Setup(GetCreateBranchExpression("Checkpoint1", "Commit1"))
				.Returns(Task.CompletedTask);

			repoClient
				.Setup(GetCreateBranchExpression("Checkpoint1Source", "Commit3"))
				.Returns(Task.CompletedTask);

			repoClient
				.Setup(GetCreatePullRequestExpression("Checkpoint1Source", "Checkpoint1"))
				.ReturnsAsync(pullRequestId);

			repoClient
				.Setup(GetDeleteBranchExpression("Checkpoint1Source"))
				.Returns(Task.CompletedTask);

			return repoClient;
		}

		/// <summary>
		/// Returns an expression for creating a branch.
		/// </summary>
		private static Expression<Func<IGitHubRepositoryClient, Task>> GetCreateBranchExpression(
			string branchName,
			string headCommit)
		{
			return repoClient => repoClient.CreateBranchAsync
			(
				"GitHubOrg", 
				"Project1_LastNameFirstName", 
				branchName, 
				headCommit
			);
		}
		
		/// <summary>
		/// Returns an expression for creating a branch.
		/// </summary>
		private static Expression<Func<IGitHubRepositoryClient, Task<int>>> GetCreatePullRequestExpression(
			string sourceBranch,
			string destBranch)
		{
			return repoClient => repoClient.CreatePullRequestAsync
			(
				"GitHubOrg",
				"Project1_LastNameFirstName",
				"Checkpoint 1 Submission",
				sourceBranch,
				destBranch
			);
		}

		/// <summary>
		/// Returns an expression for deleting a branch.
		/// </summary>
		private static Expression<Func<IGitHubRepositoryClient, Task>> GetDeleteBranchExpression(
			string branchName)
		{
			return repoClient => repoClient.DeleteBranchAsync
			(
				"GitHubOrg",
				"Project1_LastNameFirstName",
				branchName
			);
		}
	}
}
