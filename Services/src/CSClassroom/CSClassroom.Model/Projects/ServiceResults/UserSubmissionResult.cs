using System;
using System.Linq;
using CSC.CSClassroom.Model.Classrooms;

namespace CSC.CSClassroom.Model.Projects.ServiceResults
{
	/// <summary>
	/// A submission result for a student.
	/// </summary>
	public class UserSubmissionResult
	{
		/// <summary>
		/// The checkpoint name.
		/// </summary>
		public string CheckpointName { get; }

		/// <summary>
		/// The checkpoint display name.
		/// </summary>
		public string CheckpointDisplayName { get; }

		/// <summary>
		/// The checkpoint due date.
		/// </summary>
		public DateTime CheckpointDueDate { get; }

		/// <summary>
		/// The submission.
		/// </summary>
		public Submission Submission { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public UserSubmissionResult(
			Section section,
			Checkpoint checkpoint,
			Submission submission)
		{
			CheckpointName = checkpoint.Name;
			CheckpointDisplayName = checkpoint.DisplayName;
			CheckpointDueDate = checkpoint.SectionDates
				.Single(sd => sd.Section == section)
				.DueDate;
			Submission = submission;
		}
	}
}
