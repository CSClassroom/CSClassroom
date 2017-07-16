using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;
using CSC.CSClassroom.Model.Users;

namespace CSC.CSClassroom.Service.Questions.AssignmentScoring
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
