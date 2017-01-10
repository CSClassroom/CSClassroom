using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CSC.CSClassroom.WebApp.ViewModels.Project
{
	/// <summary>
	/// The view model for the CreateStudentRepositories page.
	/// </summary>
	public class CreateStudentReposViewModel
	{
		/// <summary>
		/// The project.
		/// </summary>
		public Model.Projects.Project Project { get; }

		/// <summary>
		/// The files in the project.
		/// </summary>
		public IList<ProjectRepositoryFile> Files { get; }

		/// <summary>
		/// The section options for creating repositories.
		/// </summary>
		public IList<SelectListItem> Sections { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public CreateStudentReposViewModel(Model.Projects.Project project, IList<ProjectRepositoryFile> files)
		{
			Project = project;
			Files = files;
			Sections = project.Classroom.Sections
				.Select(s => new SelectListItem() { Text = s.DisplayName, Value = s.Name })
				.ToList();
		}
	}
}
