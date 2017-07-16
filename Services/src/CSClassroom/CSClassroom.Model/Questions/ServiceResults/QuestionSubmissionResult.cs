using System;
using System.Collections.Generic;
using System.Text;

namespace CSC.CSClassroom.Model.Questions.ServiceResults
{
	/// <summary>
	/// The result for a single submission of one problem, for a single student.
	/// </summary>
	public class QuestionSubmissionResult
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
		/// The date of the submission.
		/// </summary>
		public DateTime SubmissionDate { get; }

		/// <summary>
		/// Whether or not the question submission was late.
		/// </summary>
		public SubmissionStatus Status { get; }

		/// <summary>
		/// The score for the question.
		/// </summary>
		public double Score { get; }

		/// <summary>
		/// The points the question was worth.
		/// </summary>
		public double QuestionPoints { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public QuestionSubmissionResult(
			int questionId,
			int assignmentId,
			int userId,
			DateTime submissionDate,
			SubmissionStatus status,
			double score,
			double questionPoints)
		{
			QuestionId = questionId;
			AssignmentId = assignmentId;
			UserId = userId;
			SubmissionDate = submissionDate;
			Status = status;
			Score = score;
			QuestionPoints = questionPoints;
		}
	}
}
