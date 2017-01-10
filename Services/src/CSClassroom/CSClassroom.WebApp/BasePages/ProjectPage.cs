using CSC.CSClassroom.Model.Projects;

namespace CSC.CSClassroom.WebApp.BasePages
{
	/// <summary>
	/// The base page for a view using in a particular project.
	/// </summary>
	public abstract class ProjectPage<TModel> : ClassroomPage<TModel>
	{
		/// <summary>
		/// The current project.
		/// </summary>
		public Project Project => ViewBag.Project;
	}
}
