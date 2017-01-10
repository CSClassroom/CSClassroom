using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;

namespace CSC.CSClassroom.WebApp.ViewModels.Project
{
	/// <summary>
	/// The view model for the status of all projects with one or more builds.
	/// </summary>
	public class ProjectStatusResultsViewModel
	{
		/// <summary>
		/// The user's last name.
		/// </summary>
		public string LastName { get; }

		/// <summary>
		/// The user's first name.
		/// </summary>
		public string FirstName { get; }

		/// <summary>
		/// The user's id.
		/// </summary>
		public int UserId { get; }

		/// <summary>
		/// Each project's status.
		/// </summary>
		public IList<ProjectStatusViewModel> ProjectStatus { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ProjectStatusResultsViewModel(
			ProjectStatusResults projectStatusResults)
		{
			LastName = projectStatusResults.LastName;
			FirstName = projectStatusResults.FirstName;
			UserId = projectStatusResults.UserId;
			ProjectStatus = projectStatusResults.ProjectStatus
				.Select
				(
					projectStatus => new ProjectStatusViewModel(projectStatus)
				).ToList();
		}
	}
}
