using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Assignments.ServiceResults;
using MoreLinq;

namespace CSC.CSClassroom.Service.Assignments.AssignmentScoring
{
	/// <summary>
	/// Calculates the score and status of an assignment.
	/// </summary>
	public class AssignmentScoreCalculator : IAssignmentScoreCalculator
	{
		/// <summary>
		/// The submission status calculator.
		/// </summary>
		private readonly ISubmissionStatusCalculator _submissionStatusCalculator;

		/// <summary>
		/// The time provider.
		/// </summary>
		private readonly ITimeProvider _timeProvider;

		/// <summary>
		/// Constructor.
		/// </summary>
		public AssignmentScoreCalculator(
			ISubmissionStatusCalculator submissionStatusCalculator, 
			ITimeProvider timeProvider)
		{
			_submissionStatusCalculator = submissionStatusCalculator;
			_timeProvider = timeProvider;
		}

		/// <summary>
		/// Returns the score for an assignment.
		/// </summary>
		public double GetAssignmentScore(
			IList<StudentQuestionResult> questionResults,
			int roundDigits)
		{
			return Math.Round(questionResults.Sum(qr => qr.Score), roundDigits);
		}

		/// <summary>
		/// Returns the score for an assignment.
		/// </summary>
		public double GetAssignmentScore(
			IList<AssignmentSubmissionResult> assignmentSubmissionResults,
			int roundDigits)
		{
			if (assignmentSubmissionResults.Count == 0)
				return 0.0;

			return Math.Round(assignmentSubmissionResults.Max(asr => asr.Score), roundDigits);
		}

		/// <summary>
		/// Returns the status for an assignment.
		/// </summary>
		public SubmissionStatus GetAssignmentStatus(
			IList<StudentQuestionResult> questionResults)
		{
			return _submissionStatusCalculator.GetStatusForAssignment
			(
				questionResults.Select(qr => qr.Status).ToList()
			);
		}

		/// <summary>
		/// Returns the status for an assignment.
		/// </summary>
		public SubmissionStatus GetAssignmentStatus(
			IList<AssignmentSubmissionResult> assignmentSubmissionResults,
			DateTime? dueDate)
		{
			if (assignmentSubmissionResults.Count == 0)
			{
				return new SubmissionStatus
				(
					Completion.NotStarted,
					late: dueDate.HasValue && _timeProvider.UtcNow > dueDate
				);
			}

			return assignmentSubmissionResults
				.MaxBy(asr => asr.Score)
				.First()
				.Status;
		}
	}
}
