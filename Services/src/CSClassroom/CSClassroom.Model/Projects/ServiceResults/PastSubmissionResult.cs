using System;
using CSC.CSClassroom.Model.Classrooms;

namespace CSC.CSClassroom.Model.Projects.ServiceResults
{
	/// <summary>
	/// A submission result for a student's past submission.
	/// </summary>
	public class PastSubmissionResult
	{
		/// <summary>
		/// The checkpoint display name.
		/// </summary>
		public string CheckpointDisplayName { get; }

		/// <summary>
		/// The commit date.
		/// </summary>
		public DateTime CommitDate { get; }

		/// <summary>
		/// The number of days the submission is late.
		/// </summary>
		public int DaysLate { get; }

		/// <summary>
		/// The pull request number.
		/// </summary>
		public int? PullRequestNumber { get; }

		/// <summary>
		/// Submission feedback.
		/// </summary>
		public string Feedback { get; }

		/// <summary>
		/// The build.
		/// </summary>
		public Build Build { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public PastSubmissionResult(Submission submission, Section section)
		{
			CheckpointDisplayName = submission.Checkpoint.DisplayName;
			CommitDate = submission.Commit.PushDate;
			DaysLate = submission.GetDaysLate(section);
			PullRequestNumber = submission.PullRequestNumber;
			Feedback = submission.Feedback;
			Build = submission.Commit.Build;
		}
	}
}
