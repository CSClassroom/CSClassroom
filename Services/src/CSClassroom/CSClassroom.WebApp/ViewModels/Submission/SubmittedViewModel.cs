using CSC.CSClassroom.Model.Projects;
using CSC.CSClassroom.Model.Users;
using CSC.CSClassroom.WebApp.Providers;

namespace CSC.CSClassroom.WebApp.ViewModels.Submission
{
	/// <summary>
	/// A view model for a submission just made.
	/// </summary>
	public class SubmittedViewModel
	{
		/// <summary>
		/// The user.
		/// </summary>
		public User User { get; }

		/// <summary>
		/// The checkpoint.
		/// </summary>
		public Checkpoint Checkpoint { get; }

		/// <summary>
		/// The submission just made.
		/// </summary>
		public SubmissionCandidateViewModel Submission { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public SubmittedViewModel(
			Commit commit,
			string commitUrl,
			Checkpoint checkpoint,
			ITimeZoneProvider timeZoneProvider)
		{
			User = commit.User;
			Checkpoint = checkpoint;
			Submission = new SubmissionCandidateViewModel
			(
				commit,
				commitUrl,
				true /*previousSubmission*/,
				true /*defaultChoice*/,
				timeZoneProvider
			);
		}
	}
}
