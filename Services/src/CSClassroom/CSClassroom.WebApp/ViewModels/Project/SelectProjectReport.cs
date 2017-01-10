using System.ComponentModel.DataAnnotations;

namespace CSC.CSClassroom.WebApp.ViewModels.Project
{
	/// <summary>
	/// Allows the user to select a project report.
	/// </summary>
	public class SelectProjectReport
	{
		/// <summary>
		/// The section name.
		/// </summary>
		[Display
		(
			Name = "Section",
			Description = "Select a section."
		)]
		public string SectionName { get; set; }

		/// <summary>
		/// The project name.
		/// </summary>
		[Display
		(
			Name = "Project",
			Description = "Select a project."
		)]
		public string ProjectName { get; set; }
	}
}
