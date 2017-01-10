using System;
using System.Collections.Generic;

namespace CSC.CSClassroom.Model.Questions.ServiceResults
{
	/// <summary>
	/// The result for a single student's assignment.
	/// </summary>
	public class StudentAssignmentResult
	{
		/// <summary>
		/// The name of the assignment.
		/// </summary>
		public string AssignmentName { get; }

		/// <summary>
		/// The assignment due date.
		/// </summary>
		public DateTime AssignmentDueDate { get; }

		/// <summary>
		/// The student's score for the assignment.
		/// </summary>
		public double Score { get; }

		/// <summary>
		/// The results for each question.
		/// </summary>
		public IList<StudentQuestionResult> QuestionResults { get; }

		/// <summary>
		/// The status for the overall assignment.
		/// </summary>
		public SubmissionStatus Status { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public StudentAssignmentResult(
			string assignmentName,
			DateTime assignmentDueDate,
			double score, 
			IList<StudentQuestionResult> questionResults,
			SubmissionStatus status)
		{
			AssignmentName = assignmentName;
			AssignmentDueDate = assignmentDueDate;
			Score = score;
			QuestionResults = questionResults;
			Status = status;
		}
	}
}
