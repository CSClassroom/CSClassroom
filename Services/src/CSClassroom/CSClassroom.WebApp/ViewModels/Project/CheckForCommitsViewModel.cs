using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSC.CSClassroom.WebApp.ViewModels.Project
{
	using Project = Model.Projects.Project;

	/// <summary>
	/// View model for "Check for commits" confirmation page.
	/// </summary>
    public class CheckForCommitsViewModel
	{
		/// <summary>
		/// The project.
		/// </summary>
		public Project Project { get; }

		/// <summary>
		/// The user.
		/// </summary>
		public int UserId { get; } 

		/// <summary>
		/// Constructor.
		/// </summary>
		public CheckForCommitsViewModel(Project project, int userId)
		{
			Project = project;
			UserId = userId;
		}
	}
}
