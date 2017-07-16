using System.Collections.Generic;

namespace CSC.CSClassroom.Model.Questions.ServiceResults
{
	/// <summary>
	/// The result for a single problem, for a single student.
	/// </summary>
	public class StudentQuestionResult
	{
		/// <summary>
		/// The question ID.
		/// </summary>
		public int QuestionId { get; }

		/// <summary>
		/// The assignment ID.
		/// </summary>
		public int AssignmentId { get; }

		/// <summary>
		/// The user ID.
		/// </summary>
		public int UserId { get; }

		/// <summary>
		/// Whether this question is part of an assignment 
		/// with combined submissions.
		/// </summary>
		public bool CombinedSubmissions { get; }

		/// <summary>
		/// The name of the question.
		/// </summary>
		public string QuestionName { get; }

		/// <summary>
		/// The points the question was worth.
		/// </summary>
		public double QuestionPoints { get; }

		/// <summary>
		/// The score for the question.
		/// </summary>
		public double Score { get; }

		/// <summary>
		/// The status of the question.
		/// </summary>
		public SubmissionStatus Status { get; }

		/// <summary>
		/// The result of all submissions (for non-interactive questions).
		/// </summary>
		public IList<QuestionSubmissionResult> SubmissionResults { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public StudentQuestionResult(
			int questionId,
			int assignmentId,
			int userId,
			bool combinedSubmissions,
			string questionName,
			double questionPoints,
			double score,
			SubmissionStatus status,
			IList<QuestionSubmissionResult> submissionResults)
		{
			QuestionId = questionId;
			AssignmentId = assignmentId;
			UserId = userId;
			CombinedSubmissions = combinedSubmissions;
			QuestionName = questionName;
			QuestionPoints = questionPoints;
			Score = score;
			Status = status;
			SubmissionResults = submissionResults;
		}
	}
}
