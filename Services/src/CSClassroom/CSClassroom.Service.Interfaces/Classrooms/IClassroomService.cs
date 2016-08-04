using CSC.CSClassroom.Model.Classrooms;
using System.Collections.Generic;
using System.Threading.Tasks;

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
		Task<IList<Classroom>> GetClassroomsAsync(Group group);

		/// <summary>
		/// Returns the classroom with the given name.
		/// </summary>
		Task<Classroom> GetClassroomAsync(Group group, string classroomName);

		/// <summary>
		/// Creates a classroom.
		/// </summary>
		Task CreateClassroomAsync(Group group, Classroom classroom);

		/// <summary>
		/// Updates a classroom.
		/// </summary>
		Task UpdateClassroomAsync(Group group, Classroom classroom);

		/// <summary>
		/// Removes a classroom.
		/// </summary>
		/// <param name="classroomName">The name of the classroom to remove.</param>
		Task DeleteClassroomAsync(Group group, string classroomName);
	}
}
