using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Questions.ServiceResults;

namespace CSC.CSClassroom.Service.Questions.QuestionSolvers
{
	/// <summary>
	/// Retrieves the progress of an assignment for a student.
	/// </summary>
	public interface IAssignmentProgressRetriever
	{
		/// <summary>
		/// Returns the assignment progress for a given student.
		/// </summary>
		Task<AssignmentProgress> GetAssignmentProgressAsync(
			int assignmentId,
			int currentAssignmentQuestionId,
			int userId);
	}
}
