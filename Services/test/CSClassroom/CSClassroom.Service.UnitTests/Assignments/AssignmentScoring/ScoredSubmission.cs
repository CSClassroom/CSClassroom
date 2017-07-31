using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Assignments;
using CSC.CSClassroom.Model.Assignments.ServiceResults;

namespace CSC.CSClassroom.Service.UnitTests.Assignments.AssignmentScoring
{
	/// <summary>
	/// A submission with a score.
	/// </summary>
	public class ScoredSubmission
	{
		/// <summary>
		/// The submission.
		/// </summary>
		public UserQuestionSubmission Submission { get; }

		/// <summary>
		/// The score.
		/// </summary>
		public double Score { get; }

		/// <summary>
		/// The submission status.
		/// </summary>
		public SubmissionStatus Status { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ScoredSubmission(
			UserQuestionSubmission submission,
			double score,
			bool isLate)
		{
			Submission = submission;
			Score = score;
			Status = new SubmissionStatus(Completion.Completed, isLate);
		}
	}
}
