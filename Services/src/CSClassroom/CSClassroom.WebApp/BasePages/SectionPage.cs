using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.WebApp.BasePages
{
	/// <summary>
	/// The base page for a view using in a particular section.
	/// </summary>
	public abstract class SectionPage<TModel> : ClassroomPage<TModel>
	{
		/// <summary>
		/// The current section.
		/// </summary>
		public Section Section => ViewBag.Section;

		/// <summary>
		/// The current section role.
		/// </summary>
		public SectionRole SectionRole => ViewBag.SectionRole;
	}
}
