using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Assignments.AssignmentScoring
{
	/// <summary>
	/// Generates a result for a single question completed by a student.
	/// </summary>
	public interface IQuestionResultGenerator
	{
		/// <summary>
		/// Creates a new student question result.
		/// </summary>
		StudentQuestionResult CreateQuestionResult(
			AssignmentQuestion question,
			User user,
			IList<UserQuestionSubmission> submissions,
			DateTime? dueDate);
	}
}
