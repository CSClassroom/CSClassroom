using System;
using System.Collections.Generic;
using System.Text;
using CSC.CSClassroom.Model.Questions;
using CSC.CSClassroom.Model.Questions.ServiceResults;

namespace CSC.CSClassroom.Service.UnitTests.Questions.AssignmentScoring
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
