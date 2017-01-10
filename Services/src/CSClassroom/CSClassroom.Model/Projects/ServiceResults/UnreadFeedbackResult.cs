using System;

namespace CSC.CSClassroom.Model.Projects.ServiceResults
{
	/// <summary>
	/// Unread feedback for a given project checkpoint.
	/// </summary>
	public class UnreadFeedbackResult
	{
		/// <summary>
		/// The project name.
		/// </summary>
		public string ProjectName { get; }

		/// <summary>
		/// The checkpoint name.
		/// </summary>
		public string CheckpointName { get; }

		/// <summary>
		/// The checkpoint name.
		/// </summary>
		public string CheckpointDisplayName { get; }

		/// <summary>
		/// The feedback URL.
		/// </summary>
		public int SubmissionId { get; }

		/// <summary>
		/// The commit date.
		/// </summary>
		public DateTime CommitDate { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public UnreadFeedbackResult(
			string projectName, 
			string checkpointName, 
			string checkpointDisplayName, 
			int submissionId,
			DateTime commitDate)
		{
			ProjectName = projectName;
			CheckpointName = checkpointName;
			CheckpointDisplayName = checkpointDisplayName;
			SubmissionId = submissionId;
			CommitDate = commitDate;
		}
	}
}
