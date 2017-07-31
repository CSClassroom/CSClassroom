using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSC.Common.Infrastructure.System;
using CSC.CSClassroom.Model.Questions.ServiceResults;

namespace CSC.CSClassroom.Service.Questions.AssignmentScoring
{
	/// <summary>
	/// Calculates the status of a submission for a question 
	/// or an assignment.
	/// </summary>
	public class SubmissionStatusCalculator : ISubmissionStatusCalculator
	{
		/// <summary>
		/// The time provider.
		/// </summary>
		private readonly ITimeProvider _timeProvider;

		/// <summary>
		/// Constructor.
		/// </summary>
		public SubmissionStatusCalculator(ITimeProvider timeProvider)
		{
			_timeProvider = timeProvider;
		}

		/// <summary>
		/// Returns the submission status for a question.
		/// </summary>
		public SubmissionStatus GetStatusForQuestion(
			DateTime? dateSubmitted,
			DateTime? dateDue,
			bool interactive,
			double score)
		{
			var completion = dateSubmitted != null
				? (score > 0 || !interactive)
					? Completion.Completed
					: Completion.InProgress
				: Completion.NotStarted;

			var late = IsLate(dateSubmitted, dateDue, interactive, score);

			return new SubmissionStatus(completion, late);
		}

		/// <summary>
		/// Returns the submission status for an entire assignment.
		/// </summary>
		public SubmissionStatus GetStatusForAssignment(
			IList<SubmissionStatus> questionStatus)
		{
			var completion = questionStatus.All(q => q.Completion == Completion.Completed)
				? Completion.Completed
				: questionStatus.Any(q => q.Completion != Completion.NotStarted)
					? Completion.InProgress
					: Completion.NotStarted;

			var late = questionStatus.Any(q => q.Late);

			return new SubmissionStatus(completion, late);
		}

		/// <summary>
		/// Returns whether a given submission (or non-submission) is late.
		/// </summary>
		private bool IsLate(
			DateTime? dateSubmitted,
			DateTime? dateDue,
			bool interactive,
			double score)
		{
			if (!dateDue.HasValue)
				return false;

			bool treatAsSubmitted = interactive
				? (score > 0)
				: dateSubmitted.HasValue;

			return (!treatAsSubmitted && _timeProvider.UtcNow > dateDue)
				   || (treatAsSubmitted && dateSubmitted > dateDue);
		}
	}
}
