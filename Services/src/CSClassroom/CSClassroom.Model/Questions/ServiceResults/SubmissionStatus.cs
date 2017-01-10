using System;
using System.Collections.Generic;
using System.Linq;

namespace CSC.CSClassroom.Model.Questions.ServiceResults
{
	/// <summary>
	/// The completion of a submission.
	/// </summary>
	public enum Completion
	{
		Completed,
		InProgress,
		NotStarted
	}

	/// <summary>
	/// The status of a submission.
	/// </summary>
	public class SubmissionStatus
	{
		/// <summary>
		/// The completion of the submission.
		/// </summary>
		public Completion Completion { get; }

		/// <summary>
		/// Whether or not the submission was late.
		/// </summary>
		public bool Late { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public SubmissionStatus(Completion completion, bool late)
		{
			Completion = completion;
			Late = late;
		}

		/// <summary>
		/// Returns the submission status for a question.
		/// </summary>
		public static SubmissionStatus ForQuestion(
			DateTime? dateSubmitted,
			DateTime dateDue,
			double score)
		{
			var completion = score > 0
				? Completion.Completed
				: dateSubmitted != null
					? Completion.InProgress
					: Completion.NotStarted;

			var late = score == 0 && DateTime.UtcNow >= dateDue
				|| score > 0 && dateSubmitted >= dateDue;

			return new SubmissionStatus(completion, late);
		}

		/// <summary>
		/// Returns the submission status for an entire assignment.
		/// </summary>
		public static SubmissionStatus ForAssignment(
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
	}
}
