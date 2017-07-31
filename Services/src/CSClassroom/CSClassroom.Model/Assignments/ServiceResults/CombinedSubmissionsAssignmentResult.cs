using System;
using System.Collections.Generic;
using System.Text;

namespace CSC.CSClassroom.Model.Questions.ServiceResults
{
	/// <summary>
	/// An assignment result for an assignment with questions that are 
	/// answered in a single submission.
	/// </summary>
	public class CombinedSubmissionsAssignmentResult : AssignmentResult
	{
		/// <summary>
		/// The results for each assignment submission.
		/// </summary>
		public IList<AssignmentSubmissionResult> AssignmentSubmissionResults { get; }

		/// <summary>
		/// Whether or not submissions for this assignment are combined.
		/// </summary>
		public override bool CombinedSubmissions => true;

		/// <summary>
		/// Constructor.
		/// </summary>
		public CombinedSubmissionsAssignmentResult(
			string assignmentName,
			int assignmentId,
			int userId,
			DateTime? assignmentDueDate,
			double score,
			double totalPoints,
			SubmissionStatus status,
			IList<AssignmentSubmissionResult> assignmentSubmissionResults) :
				base
				(
					assignmentName,
					assignmentId,
					userId,
					assignmentDueDate,
					score,
					totalPoints,
					status
				)
		{
			AssignmentSubmissionResults = assignmentSubmissionResults;
		}
	}
}
