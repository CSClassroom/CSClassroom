using System;
using System.Collections.Generic;
using System.Text;

namespace CSC.CSClassroom.Model.Assignments.ServiceResults
{
	/// <summary>
	/// An assignment result for an assignment with questions that are 
	/// answered separately.
	/// </summary>
	public class SeparateSubmissionsAssignmentResult : AssignmentResult
	{
		/// <summary>
		/// The results for each question.
		/// </summary>
		public IList<StudentQuestionResult> QuestionResults { get; }

		/// <summary>
		/// Whether or not submissions for this assignment are combined.
		/// </summary>
		public override bool CombinedSubmissions => false;

		/// <summary>
		/// Constructor.
		/// </summary>
		public SeparateSubmissionsAssignmentResult(
			string assignmentName,
			int assignmentId,
			int userId,
			DateTime? assignmentDueDate,
			double score,
			double totalPoints,
			SubmissionStatus status,
			IList<StudentQuestionResult> questionResults) :
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
			QuestionResults = questionResults;
		}
	}
}
