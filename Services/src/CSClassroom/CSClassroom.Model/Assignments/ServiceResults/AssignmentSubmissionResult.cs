using System;
using System.Collections.Generic;
using System.Text;

namespace CSC.CSClassroom.Model.Questions.ServiceResults
{
	/// <summary>
	/// The result for a single submission for one assignment with combined 
	/// submissions, for a single student.
	/// </summary>
	public class AssignmentSubmissionResult
	{
		/// <summary>
		/// The assignment ID.
		/// </summary>
		public int AssignmentId { get; }

		/// <summary>
		/// The user ID.
		/// </summary>
		public int UserId { get; }
		
		/// <summary>
		/// The date of the submission.
		/// </summary>
		public DateTime SubmissionDate { get; }

		/// <summary>
		/// The status of the submission.
		/// </summary>
		public SubmissionStatus Status { get; }

		/// <summary>
		/// The score for the submission.
		/// </summary>
		public double Score { get; }

		/// <summary>
		/// The points the assignment was worth.
		/// </summary>
		public double AssignmentPoints { get; }

		/// <summary>
		/// The question results for this submission.
		/// </summary>
		public IList<StudentQuestionResult> QuestionResults { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentSubmissionResult(
			int assignmentId,
			int userId,
			DateTime submissionDate,
			SubmissionStatus status,
			double score,
			double assignmentPoints,
			IList<StudentQuestionResult> questionResults)
		{
			AssignmentId = assignmentId;
			UserId = userId;
			SubmissionDate = submissionDate;
			Status = status;
			Score = score;
			AssignmentPoints = assignmentPoints;
			QuestionResults = questionResults;
		}
	}
}
