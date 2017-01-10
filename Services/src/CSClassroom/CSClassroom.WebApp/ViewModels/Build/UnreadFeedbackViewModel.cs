using System;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Projects.ServiceResults;
using CSC.CSClassroom.WebApp.Extensions;
using CSC.CSClassroom.WebApp.Providers;

namespace CSC.CSClassroom.WebApp.ViewModels.Build
{
	/// <summary>
	/// The view model for unread feedback.
	/// </summary>
	public class UnreadFeedbackViewModel
	{
		/// <summary>
		/// The project name.
		/// </summary>
		public string ProjectName { get; }

		/// <summary>
		/// The checkpoint name.
		/// </summary>
		public string CheckpointDisplayName { get; }

		/// <summary>
		/// The commit date.
		/// </summary>
		public string CommitDate { get; }

		/// <summary>
		/// The feedback URL.
		/// </summary>
		public string FeedbackUrl { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public UnreadFeedbackViewModel(
			UnreadFeedbackResult result,
			Func<string, string, int, string> feedbackUrlBuilder,
			ITimeZoneProvider timeZoneProvider)
		{
			ProjectName = result.ProjectName;
			CheckpointDisplayName = result.CheckpointDisplayName;
			CommitDate = result.CommitDate.FormatLongDateTime(timeZoneProvider);
			FeedbackUrl = feedbackUrlBuilder(result.ProjectName, result.CheckpointName, result.SubmissionId);
		}
	}
}
