using System;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;
using CSC.CSClassroom.WebApp.Extensions;
using CSC.CSClassroom.WebApp.Providers;

namespace CSC.CSClassroom.WebApp.ViewModels.Build
{
	/// <summary>
	/// The status of a checkpoint for a student.
	/// </summary>
	public class CheckpointSubmissionViewModel
	{
		/// <summary>
		/// The checkpoint
		/// </summary>
		public string CheckpointName { get; }

		/// <summary>
		/// The checkpoint
		/// </summary>
		public string CheckpointDisplayName { get; }

		/// <summary>
		/// The due date for the checkpoint.
		/// </summary>
		public string CheckpointDueDate { get; }

		/// <summary>
		/// The submission ID.
		/// </summary>
		public int? SubmissionId { get; }

		/// <summary>
		/// The date of the submission, if a submission has been made.
		/// </summary>
		public string SubmissionDate { get; }

		/// <summary>
		/// Whether or not the submission is past due.
		/// </summary>
		public bool PastDue { get; }

		/// <summary>
		/// Whether or not feedback is available.
		/// </summary>
		public bool FeedbackAvailable { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public CheckpointSubmissionViewModel(
			UserSubmissionResult submissionResult,
			ITimeZoneProvider timeZoneProvider)
		{
			CheckpointName = submissionResult.CheckpointName;
			CheckpointDisplayName = submissionResult.CheckpointDisplayName;
			CheckpointDueDate = submissionResult.CheckpointDueDate
				.FormatLongDateTime(timeZoneProvider);

			SubmissionId = submissionResult.Submission?.Id;

			if (submissionResult.Submission?.DateSubmitted != null)
			{
				SubmissionDate = submissionResult.Submission
					.DateSubmitted
					.FormatLongDateTime(timeZoneProvider);
			}

			PastDue = submissionResult.Submission == null 
				&& submissionResult.CheckpointDueDate < DateTime.UtcNow;

			FeedbackAvailable = submissionResult.Submission?.FeedbackSent ?? false;
		}
	}
}
