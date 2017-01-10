using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.BuildService.Model.ProjectRunner;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;
using TestResult = CSC.CSClassroom.Model.Projects.TestResult;

namespace CSC.CSClassroom.Service.Projects
{
	/// <summary>
	/// Performs build operations.
	/// </summary>
	public interface IBuildService
	{
		/// <summary>
		/// Returns the list of builds for a given user/project.
		/// </summary>
		Task<IList<Build>> GetUserBuildsAsync(
			string classroomName, 
			string projectName, 
			int userId);

		/// <summary>
		/// Returns the latest build for each student in the given section,
		/// for a given project.
		/// </summary>
		Task<IList<Build>> GetSectionBuildsAsync(
			string classroomName,
			string projectName,
			string sectionName);

		/// <summary>
		/// Returns the latest build for the given user and project.
		/// </summary>
		Task<LatestBuildResult> GetLatestBuildResultAsync(
			string classroomName, 
			string projectName, 
			int userId);

		/// <summary>
		/// Returns the build with the given id.
		/// </summary>
		Task<BuildResult> GetBuildResultAsync(
			string classroomName,
			string projectName, 
			int buildId);

		/// <summary>
		/// Called by the build service when a build completes, to store the result.
		/// </summary>
		Task OnBuildCompletedAsync(ProjectJobResult buildJobResult);

		/// <summary>
		/// Monitors the progress of a job.
		/// </summary>
		Task<BuildProgress> MonitorProgressAsync(
			string classroomName, 
			string projectName, 
			int userId);

		/// <summary>
		/// Returns a single test result.
		/// </summary>
		Task<TestResult> GetTestResultAsync(
			string classroomName, 
			string projectName, 
			int testResultId);
	}
}
