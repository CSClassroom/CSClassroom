using System;
using System.Linq;
using CSC.CSClassroom.Model.Classrooms;

namespace CSC.CSClassroom.Model.Projects
{
	/// <summary>
	/// A submission of a project.
	/// </summary>
	public class Submission
	{
		/// <summary>
		/// The ID of the build.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The ID of the commit that was submitted.
		/// </summary>
		public int CommitId { get; set; }

		/// <summary>
		/// The commit that was submitted.
		/// </summary>
		public Commit Commit { get; set; }

		/// <summary>
		/// The checkpoint the submission is being made for.
		/// </summary>
		public int CheckpointId { get; set; }

		/// <summary>
		/// The checkpoint the submission is being made for.
		/// </summary>
		public Checkpoint Checkpoint { get; set; }

		/// <summary>
		/// The GitHub pull request number for this submission.
		/// </summary>
		public int PullRequestNumber { get; set; }

		/// <summary>
		/// The date the submission was made.
		/// </summary>
		public DateTime DateSubmitted { get; set; }

		/// <summary>
		/// The feedback for this submission.
		/// </summary>
		public string Feedback { get; set; }

		/// <summary>
		/// The date this feedback was saved.
		/// </summary>
		public DateTime? DateFeedbackSaved { get; set; }

		/// <summary>
		/// Whether or not the feedback was sent to the student.
		/// </summary>
		public bool FeedbackSent { get; set; }

		/// <summary>
		/// The date the feedback was read by the student.
		/// </summary>
		public DateTime? DateFeedbackRead { get; set; }

		/// <summary>
		/// Returns the number of days late a given submission is.
		/// </summary>
		public int GetDaysLate(Section section)
		{
			var dueDate = Checkpoint
				.SectionDates
				.Single(sd => sd.Section == section)
				.DueDate;

			return (int)Math.Ceiling
			(
				Math.Max
				(
					(Commit.PushDate - dueDate).TotalDays,
					0.0
				)
			);
		}
	}
}
