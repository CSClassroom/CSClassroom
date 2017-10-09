using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;

namespace CSC.CSClassroom.Service.Projects
{
	/// <summary>
	/// Performs project operations.
	/// </summary>
	public interface IProjectService
	{
		/// <summary>
		/// Returns the list of projects.
		/// </summary>
		Task<IList<Project>> GetProjectsAsync(string classroomName);

		/// <summary>
		/// Returns the project with the given name.
		/// </summary>
		Task<Project> GetProjectAsync(string classroomName, string projectName);

		/// <summary>
		/// Creates a project.
		/// </summary>
		Task CreateProjectAsync(string classroomName, Project project);

		/// <summary>
		/// Updates a project.
		/// </summary>
		Task UpdateProjectAsync(string classroomName, Project project);

		/// <summary>
		/// Removes a project.
		/// </summary>
		Task DeleteProjectAsync(string classroomName, string projectName);

		/// <summary>
		/// Returns a list of files in a project template repository.
		/// </summary>
		Task<IList<ProjectRepositoryFile>> GetTemplateFileListAsync(
			string classroomName,
			string projectName);

		/// <summary>
		/// Creates student repositories for all students in a given section,
		/// based on the contents of the project template.
		/// </summary>
		Task<IList<CreateStudentRepoResult>> CreateStudentRepositoriesAsync(
			string classroomName,
			string projectName,
			string sectionName,
			string webhookUrl,
			bool overwriteIfSafe);

		/// <summary>
		/// Verifies that a GitHub webhook payload is correctly signed.
		/// </summary>
		bool VerifyGitHubWebhookPayloadSigned(byte[] content, string signature);

		/// <summary>
		/// Called when a push event is received from a GitHub web hook,
		/// to queue a build of the commit that was pushed.
		/// </summary>
		Task OnRepositoryPushAsync(
			string classroomName,
			string serializedPushEvent,
			string buildResultCallbackUrl);

		/// <summary>
		/// Checks for missed push events for all students.
		/// Returns false if the project does not exist.
		/// </summary>
		Task<bool> ProcessMissedCommitsForAllStudentsAsync(
			string classroomName,
			string projectName,
			string buildResultCallbackUrl);

		/// <summary>
		/// Checks for missed push events for a single student.
		/// Returns false if the project or student does not exist.
		/// </summary>
		Task<bool> ProcessMissedCommitsForStudentAsync(
			string classroomName,
			string projectName,
			int userId,
			string buildResultCallbackUrl);

		/// <summary>
		/// Returns the project status for each active project,
		/// for the given user.
		/// </summary>
		Task<ProjectStatusResults> GetProjectStatusAsync(
			string classroomName,
			int userId);
	}
}
