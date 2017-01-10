using System;
using System.Collections.Generic;
using System.Linq;
using CSC.CSClassroom.Model.Classrooms;

namespace CSC.CSClassroom.Model.Projects.ServiceResults
{
	/// <summary>
	/// The feedback for a submission.
	/// </summary>
	public class ViewFeedbackResult
	{
		/// <summary>
		/// The student's last name.
		/// </summary>
		public string LastName { get; }

		/// <summary>
		/// The student's first name.
		/// </summary>
		public string FirstName { get; }

		/// <summary>
		/// The user ID.
		/// </summary>
		public int UserId { get; }

		/// <summary>
		/// The repository name.
		/// </summary>
		public string RepoName { get; }

		/// <summary>
		/// The submission ID.
		/// </summary>
		public int SubmissionId { get; }

		/// <summary>
		/// The pull request number for the submission.
		/// </summary>
		public int PullRequestNumber { get; }

		/// <summary>
		/// The push date.
		/// </summary>
		public DateTime PushDate { get; }

		/// <summary>
		/// The feedback.
		/// </summary>
		public string Feedback { get; }

		/// <summary>
		/// Whether or not the feedback was unread.
		/// </summary>
		public bool Unread { get; }

		/// <summary>
		/// Whether or not there are future checkpoints.
		/// </summary>
		public bool FutureCheckpoints { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ViewFeedbackResult(
			Section section,
			Submission submission,
			IList<Checkpoint> checkpoints)
		{
			LastName = submission.Commit.User.LastName;
			FirstName = submission.Commit.User.FirstName;
			UserId = submission.Commit.User.Id;
			RepoName = submission.Commit.GetRepoName();
			SubmissionId = submission.Id;
			PullRequestNumber = submission.PullRequestNumber;
			PushDate = submission.Commit.PushDate;
			Feedback = submission.Feedback;
			Unread = submission.DateFeedbackRead == null;

			var dueDate = submission.Checkpoint
				.SectionDates
				?.FirstOrDefault(sd => sd.Section == section)
				?.DueDate;

			FutureCheckpoints = dueDate != null 
				&& submission.Checkpoint
					.Project
					.Checkpoints
					.Any
					(
						c => c.SectionDates?.Any
						(
							sd => sd.SectionId == section.Id && sd.DueDate > dueDate.Value
						) ?? false
					);
		}
	}
}
