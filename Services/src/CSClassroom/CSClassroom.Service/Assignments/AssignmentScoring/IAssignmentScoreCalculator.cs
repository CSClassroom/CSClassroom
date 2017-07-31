using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Questions.ServiceResults;

namespace CSC.CSClassroom.Service.Questions.AssignmentScoring
{
	/// <summary>
	/// Calculates the score and status of an assignment.
	/// </summary>
	public interface IAssignmentScoreCalculator
	{
		/// <summary>
		/// Returns the score for an assignment.
		/// </summary>
		double GetAssignmentScore(
			IList<StudentQuestionResult> questionResults,
			int roundDigits);

		/// <summary>
		/// Returns the score for an assignment.
		/// </summary>
		double GetAssignmentScore(
			IList<AssignmentSubmissionResult> assignmentSubmissionResults,
			int roundDigits);

		/// <summary>
		/// Returns the status for an assignment.
		/// </summary>
		SubmissionStatus GetAssignmentStatus(
			IList<StudentQuestionResult> questionResults);

		/// <summary>
		/// Returns the status for an assignment.
		/// </summary>
		SubmissionStatus GetAssignmentStatus(
			IList<AssignmentSubmissionResult> assignmentSubmissionResults,
			DateTime? dueDate);
	}
}
