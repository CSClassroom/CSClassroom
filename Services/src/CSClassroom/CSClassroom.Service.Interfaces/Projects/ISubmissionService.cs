using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CSC.CSClassroom.Model.Classrooms;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;

namespace CSC.CSClassroom.Service.Projects
{
	/// <summary>
	/// Performs submission operations.
	/// </summary>
	public interface ISubmissionService
	{
		/// <summary>
		/// Returns a list of submission candidates.
		/// </summary>
		Task<IList<Commit>> GetSubmissionCandidatesAsync(
			string classroomName,
			string projectName,
			int userId);

		/// <summary>
		/// Returns the list of submissions a user has made for a given project.
		/// </summary>
		Task<IList<Submission>> GetUserSubmissionsAsync(
			string classroomName,
			string projectName,
			int userId);

		/// <summary>
		/// Returns the latest submission for each student in a given section,
		/// for a given checkpoint.
		/// </summary>
		Task<IList<SectionSubmissionResult>> GetSectionSubmissionsAsync(
			string classroomName,
			string projectName,
			string checkpointName,
			string sectionName);

		/// <summary>
		/// Submits a checkpoint.
		/// </summary>
		Task<Submission> SubmitCheckpointAsync(
			string classroomName,
			string projectName,
			string checkpointName,
			int userId,
			int commitId);

		/// <summary>
		/// Downloads all submissions for a given checkpoint in a section,
		/// in the form of a zip archive.
		/// </summary>
		Task<Stream> DownloadSubmissionsAsync(
			string classroomName,
			string projectName,
			string checkpointName,
			string sectionName);

		/// <summary>
		/// Returns submissions for grading.
		/// </summary>
		Task<IList<GradeSubmissionResult>> GradeSubmissionsAsync(
			string classroomName,
			string projectName,
			string checkpointName,
			string sectionName);

		/// <summary>
		/// Updates submission feedback.
		/// </summary>
		Task SaveFeedbackAsync(
			string classroomName,
			string projectName,
			string checkpointName,
			int submissionId,
			string feedbackText);

		/// <summary>
		/// Sends all filled-out feedback for the given checkpoint in the given section.
		/// </summary>
		Task SendFeedbackAsync(
			string classroomName,
			string projectName,
			string checkpointName,
			string sectionName,
			string fromAddress,
			Func<Submission, string> viewFeedbackUrlBuilder);

		/// <summary>
		/// Returns the feedback for the given submission.
		/// </summary>
		Task<ViewFeedbackResult> GetSubmissionFeedbackAsync(
			string classroomName,
			string projectName,
			string checkpointName,
			int submissionId);

		/// <summary>
		/// Marks feedback as read for the given submission.
		/// </summary>
		Task MarkFeedbackReadAsync(
			string classroomName,
			string projectName,
			string checkpointName,
			int submissionId,
			int userId);

		/// <summary>
		/// Returns all unread feedback for the given user.
		/// </summary>
		Task<IList<UnreadFeedbackResult>> GetUnreadFeedbackAsync(int userId);
	}
}
