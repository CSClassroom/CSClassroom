using System.Collections.Generic;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Classrooms
{
	/// <summary>
	/// Performs classroom operations.
	/// </summary>
	public interface IClassroomService
	{
		/// <summary>
		/// Returns the list of classrooms.
		/// </summary>
		Task<IList<Classroom>> GetClassroomsAsync();

		/// <summary>
		/// Returns the classroom with the given name.
		/// </summary>
		Task<Classroom> GetClassroomAsync(string classroomName);

		/// <summary>
		/// Returns all administrators of the current classroom.
		/// </summary>
		Task<IList<ClassroomMembership>> GetClassroomAdminsAsync(string classroomName);

		/// <summary>
		/// Returns all classrooms the user has access to.
		/// </summary>
		Task<IList<ClassroomMembership>> GetClassroomsWithAccessAsync(int userId);

		/// <summary>
		/// Creates a classroom.
		/// </summary>
		Task CreateClassroomAsync(Classroom classroom);

		/// <summary>
		/// Updates a classroom.
		/// </summary>
		Task UpdateClassroomAsync(Classroom classroom);

		/// <summary>
		/// Removes a classroom.
		/// </summary>
		/// <param name="classroomName">The name of the classroom to remove.</param>
		Task DeleteClassroomAsync(string classroomName);
	}
}
