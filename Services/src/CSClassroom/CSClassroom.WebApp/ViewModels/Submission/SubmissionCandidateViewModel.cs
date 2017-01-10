using System.Linq;
using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.WebApp.Extensions;
using CSC.CSClassroom.WebApp.Providers;

namespace CSC.CSClassroom.WebApp.ViewModels.Submission
{
	/// <summary>
	/// A view model for a submission candidate.
	/// </summary>
	public class SubmissionCandidateViewModel
	{
		/// <summary>
		/// The ID of the commit for this submission candidate.
		/// </summary>
		public int CommitId { get; }

		/// <summary>
		/// The Sha of the commit.
		/// </summary>
		public string Sha { get; }

		/// <summary>
		/// The date the commit was made.
		/// </summary>
		public string DateCommitted { get; }

		/// <summary>
		/// The GitHub URL for the commit.
		/// </summary>
		public string GitHubUrl { get; }

		/// <summary>
		/// The ID of the build for this submission candidate.
		/// </summary>
		public int BuildId { get; }

		/// <summary>
		/// The number of passing tests.
		/// </summary>
		public int? PassingTests { get; }

		/// <summary>
		/// The commit message.
		/// </summary>
		public string CommitMessage { get; }

		/// <summary>
		/// Whether or not this was the previous submission.
		/// </summary>
		public bool PreviousSubmission { get; }

		/// <summary>
		/// Whether or not this is the default choice for submission.
		/// </summary>
		public bool DefaultChoice { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public SubmissionCandidateViewModel(
			Commit commit,
			string commitUrl, 
			bool previousSubmission,
			bool defaultChoice,
			ITimeZoneProvider timeZoneProvider)
		{
			CommitId = commit.Id;
			Sha = commit.Sha;
			DateCommitted = commit.PushDate.FormatLongDateTime(timeZoneProvider);
			GitHubUrl = commitUrl;
			BuildId = commit.Build.Id;
			PassingTests = commit.Build.TestResults.Count(t => t.Succeeded);
			CommitMessage = commit.Message;
			PreviousSubmission = previousSubmission;
			DefaultChoice = defaultChoice;
		}
	}
}
