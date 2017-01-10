using System.Collections.Generic;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.WebApp.BasePages
{
	/// <summary>
	/// The base page for a view using in a particular classroom.
	/// </summary>
	public abstract class ClassroomPage<TModel> : BasePage<TModel>
	{
		/// <summary>
		/// The current classroom.
		/// </summary>
		public Classroom Classroom => ViewBag.Classroom;

		/// <summary>
		/// The current classroom role.
		/// </summary>
		public ClassroomRole ClassroomRole => ViewBag.ClassroomRole;

		/// <summary>
		/// The classrooms with access.
		/// </summary>
		public IList<ClassroomMembership> ClassroomsWithAccess => ViewBag.ClassroomsWithAccess;
	}
}
