using CSC.CSClassroom.Model.Assignments;

namespace CSC.CSClassroom.WebApp.BasePages
{
	/// <summary>
	/// The base page for a view using in a particular checkpoint.
	/// </summary>
	public abstract class AssignmentPage<TModel> : ClassroomPage<TModel>
	{
		/// <summary>
		/// The current checkpoint.
		/// </summary>
		public Assignment Assignment => ViewBag.Assignment;
	}
}
