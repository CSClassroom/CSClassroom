using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSC.CSClassroom.Service.Assignments.QuestionSolvers
{
	/// <summary>
	/// Retrieves an assignment due date from the database.
	/// </summary>
	public interface IAssignmentDueDateRetriever
	{
		/// <summary>
		/// Returns the due date for the assignment, for the given user.
		/// </summary>
		Task<DateTime?> GetUserAssignmentDueDateAsync(
			string classroomName,
			int assignmentId,
			int userId);
	}
}
