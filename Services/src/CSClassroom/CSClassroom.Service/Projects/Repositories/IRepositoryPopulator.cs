using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Projects.Repositories
{
	/// <summary>
	/// Populates student repositories.
	/// </summary>
	public interface IRepositoryPopulator
	{
		/// <summary>
		/// Retunrs a list of files in a project repository.
		/// </summary>
		Task<IList<ProjectRepositoryFile>> GetRepoFileListAsync(
			Project project);

		/// <summary>
		/// Creates repositories for the given students, and pushes the non-private
		/// files from the source project to the new repository.
		/// </summary>
		Task<IList<CreateStudentRepoResult>> CreateReposAsync(
			Project project,
			IList<ClassroomMembership> students,
			string webhookUrl,
			bool overwriteIfSafe);

		/// <summary>
		/// Ensures that webhooks are present in all student repositories.
		/// </summary>
		Task EnsureWebHooksPresentAsync(
			Project project,
			IList<ClassroomMembership> students,
			string webhookUrl);
	}
}
