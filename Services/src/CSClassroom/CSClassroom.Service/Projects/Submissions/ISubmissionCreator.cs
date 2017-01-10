using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Projects.Submissions
{
	/// <summary>
	/// Creates submissions for students.
	/// </summary>
	public interface ISubmissionCreator
	{
		/// <summary>
		/// Returns a list of all valid submission commit SHAs for the given user/project.
		/// </summary>
		Task<ICollection<string>> GetSubmissionCandidatesAsync(
			Project project,
			User user);

		/// <summary>
		/// Populates a submission branch with a pull request, and returns the 
		/// pull request number.
		/// </summary>
		Task<int> CreatePullRequestAsync(
			Commit commit,
			Checkpoint checkpoint);
	}
}
