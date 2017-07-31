using System;
using System.Collections.Generic;

namespace CSC.CSClassroom.Model.Assignments.ServiceResults
{
	/// <summary>
	/// The result for a single student's assignment.
	/// </summary>
	public abstract class AssignmentResult
	{
		/// <summary>
		/// The name of the assignment.
		/// </summary>
		public string AssignmentName { get; }

		/// <summary>
		/// The assignment ID.
		/// </summary>
		public int AssignmentId { get; }

		/// <summary>
		/// The user ID.
		/// </summary>
		public int UserId { get; }

		/// <summary>
		/// The assignment due date.
		/// </summary>
		public DateTime? AssignmentDueDate { get; }

		/// <summary>
		/// The student's score for the assignment.
		/// </summary>
		public double Score { get; }

		/// <summary>
		/// The total points the assignment is out of.
		/// </summary>
		public double TotalPoints { get; }

		/// <summary>
		/// The status for the overall assignment.
		/// </summary>
		public SubmissionStatus Status { get; }

		/// <summary>
		/// Whether or not submissions for this assignment are combined.
		/// </summary>
		public abstract bool CombinedSubmissions { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AssignmentResult(
			string assignmentName,
			int assignmentId,
			int userId,
			DateTime? assignmentDueDate,
			double score,
			double totalPoints,
			SubmissionStatus status)
		{
			AssignmentName = assignmentName;
			AssignmentId = assignmentId;
			UserId = userId;
			AssignmentDueDate = assignmentDueDate;
			Score = score;
			TotalPoints = totalPoints;
			Status = status;
		}
	}
}
