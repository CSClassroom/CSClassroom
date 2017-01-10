using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.Common.Infrastructure.GitHub;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Projects.Repositories
{
	/// <summary>
	/// Retrieves metadata about project repositories.
	/// </summary>
	public interface IRepositoryMetadataRetriever
	{
		/// <summary>
		/// Retrieves a list of student repositories for a given project.
		/// </summary>
		Task<IDictionary<ClassroomMembership, GitHubRepository>> GetStudentRepositoriesAsync(
			Project project,
			IList<ClassroomMembership> students);

		/// <summary>
		/// Returns the repository name for the given student/project.
		/// </summary>
		string GetRepoName(Project project, ClassroomMembership student);
	}
}