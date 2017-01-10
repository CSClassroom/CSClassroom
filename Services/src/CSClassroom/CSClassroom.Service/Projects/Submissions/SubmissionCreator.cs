using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.GitHub;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Projects.Submissions
{
	/// <summary>
	/// GitHub operations related to a submission of a commit.
	/// </summary>
	public class SubmissionCreator : ISubmissionCreator
	{
		/// <summary>
		/// The repository client.
		/// </summary>
		private readonly IGitHubRepositoryClient _repoClient;

		/// <summary>
		/// Constructor.
		/// </summary>
		public SubmissionCreator(IGitHubRepositoryClient repoClient)
		{
			_repoClient = repoClient;
		}

		/// <summary>
		/// Returns a list of all valid submission commit SHAs for the given user/project.
		/// </summary>
		public async Task<ICollection<string>> GetSubmissionCandidatesAsync(
			Project project,
			User user)
		{
			var allCommits = await GetAllCommitsAsync
			(
				project,
				GetStudent(project, user)	
			);

			return new HashSet<string>
			(
				allCommits
					.Where(c => c.Parents.Count > 0)
					.Select(commit => commit.Sha)
			);
		}

		/// <summary>
		/// Populates a submission branch with a pull request, and returns the 
		/// pull request number.
		/// </summary>
		public async Task<int> CreatePullRequestAsync(
			Commit commit,
			Checkpoint checkpoint)
		{
			var student = GetStudent(commit.Project, commit.User);
			var orgName = student.Classroom.GitHubOrganization;
			var repoName = checkpoint.Project.GetStudentRepoName(student);

			// Get the commits to use in the pull requests.
			var allCommits = await GetAllCommitsAsync(commit.Project, student);
			var submissionCommit = allCommits.SingleOrDefault(c => c.Sha == commit.Sha);
			var startingCommit = allCommits.Where(c => c.Parents.Count == 0)
				.OrderBy(c => c.Date)
				.FirstOrDefault();

			// Create a submission branch that initially just contains the starting commit.
			// This will be the destination of our new pull request.
			var destBranchName = checkpoint.Name;
			await _repoClient.CreateBranchAsync
			(
				orgName, 
				repoName, 
				destBranchName, 
				startingCommit.Sha
			);

			// Create a temporary source branch that initially contains the student's commit 
			// for the checkpoint, and all previous commits. This is the source of the pull request.
			var sourceBranchName = $"{checkpoint.Name}Source";
			await _repoClient.CreateBranchAsync
			(
				orgName,
				repoName,
				sourceBranchName,
				submissionCommit.Sha
			);

			// Create the pull request.
			var pullRequestTitle = $"{checkpoint.DisplayName} Submission";
			int pullRequestNumber = await _repoClient.CreatePullRequestAsync
			(
				orgName,
				repoName,
				pullRequestTitle,
				sourceBranchName,
				destBranchName
			);

			// Delete the temporary source branch.
			await _repoClient.DeleteBranchAsync(orgName, repoName, sourceBranchName);

			return pullRequestNumber;
		}
		
		/// <summary>
		/// Returns the student classroom membership for the user.
		/// </summary>
		private ClassroomMembership GetStudent(Project project, User user)
		{
			return user.ClassroomMemberships.Single
			(
				cm => cm.Classroom == project.Classroom
			);
		}

		/// <summary>
		/// Returns all commits for the given user/project.
		/// </summary>
		private async Task<ICollection<GitHubCommit>> GetAllCommitsAsync(
			Project project, 
			ClassroomMembership student)
		{
			var orgName = student.Classroom.GitHubOrganization;
			var repoName = project.GetStudentRepoName(student);

			return await _repoClient.GetAllCommitsAsync(orgName, repoName);
		}
	}
}
